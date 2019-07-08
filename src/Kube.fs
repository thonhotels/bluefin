namespace Bluefin

open Fake.Core
open System.Text.RegularExpressions

module Kube =
    [<NoComparison>]
    [<NoEquality>]
    type ExecProcessOptions = {
        DisplayName: string
        WorkingDirectory: string
        RedactTrace: string -> string
    }

    let defaultOptions = { DisplayName = ""; WorkingDirectory = ""; RedactTrace = id }

    let private execProcess name arguments createOptions resultFn =

        // if not(System.Diagnostics.Debugger.IsAttached) then
        //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
        // while not(System.Diagnostics.Debugger.IsAttached) do
        //   System.Threading.Thread.Sleep(100)
        // System.Diagnostics.Debugger.Break() 
            
        let joinArgs = String.concat " " 

        let options = createOptions defaultOptions
        
        let cli = ProcessUtils.tryFindFileOnPath name
                    |> function 
                    | Some cli -> cli
                    | None -> failwithf "Can't find %s on path" (if String.isNotNullOrEmpty options.DisplayName then name else options.DisplayName)      

        try 
            CreateProcess.fromRawCommand cli (arguments)
            |> CreateProcess.withWorkingDirectory options.WorkingDirectory
            |> CreateProcess.redirectOutput
            |> CreateProcess.disableTraceCommand
            |> CreateProcess.addOnSetup (fun () -> 
                        Trace.tracefn "%s> \"%s\" %s \n" options.WorkingDirectory cli (joinArgs >> options.RedactTrace <| arguments)
            )
            |> Proc.run
            |> resultFn 
          
        with ex ->
            failwithf "Error calling %s %s  dir: %s \n %O" name (joinArgs >> options.RedactTrace <| arguments) options.WorkingDirectory ex      

    let private execProcessString name arguments createOptions =
        execProcess name arguments createOptions (fun res -> 
                        if res.ExitCode <> 0 then failwithf "Step failed: %O" res.Result.Error
                        res.Result.Output.Trim().Trim('"'))

    let argsToArray args = // Split a command on whitespace, ignoring quoted sections
          let regex = Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)", RegexOptions.Multiline)
          regex.Split(args)
          |> Array.filter(String.isNullOrWhiteSpace >> not)

    let kube arguments =
        execProcessString "kubectl" (argsToArray arguments) (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = "" }) 

    let kubeArr arguments =
        execProcessString "kubectl" arguments (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = "" })

    let kubeWorkingDir arguments workingDirectory =
        execProcessString "kubectl" (argsToArray arguments) (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = workingDirectory })

    let kubeRedact arguments redactedArgs =
        execProcessString "kubectl" (argsToArray arguments) (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = ""; RedactTrace = fun _ -> redactedArgs }) 

    let redactValue cmd secret = 
        cmd 
        |> String.replace secret "****" 

    let exec arguments = kube arguments |> ignore 

    let kubeResult arguments = 
        let r = execProcess "kubectl" (argsToArray arguments) (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = "" }) id

        { r with Result = { r.Result with Output = r.Result.Output.Trim()}}
