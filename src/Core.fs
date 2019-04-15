namespace Bluefin

open Fake.Core
open System.Text.RegularExpressions

module Core =
    [<NoComparison>]
    [<NoEquality>]
    type ExecProcessOptions = {
        DisplayName: string
        WorkingDirectory: string
        RedactTrace: string -> string
    }

    let defaultOptions = { DisplayName = ""; WorkingDirectory = ""; RedactTrace = id }

    let private execProcess name arguments createOptions =

          let arguments = // Split a command on whitespace, ignoring quoted sections
            let regex = Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)", RegexOptions.Multiline)
            regex.Split(arguments)
            |> Array.filter(String.isNullOrWhiteSpace >> not)     

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
              |> fun res -> 
                        if res.ExitCode <> 0 then failwithf "Step failed: %O" res.Result.Error
                        res.Result.Output.Trim().Trim('"')     
            
          with ex ->
              failwithf "Error calling %s %s  dir: %s \n %O" name (joinArgs >> options.RedactTrace <| arguments) options.WorkingDirectory ex      

    let az arguments =
        execProcess "az" arguments (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = "" }) 

    let azWorkingDir arguments workingDirectory =
        execProcess "az" arguments (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = workingDirectory })

    let azRedact arguments redactedArgs =
        execProcess "az" arguments (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = ""; RedactTrace = fun _ -> redactedArgs }) 

    let redactValue cmd secret = 
        cmd 
        |> String.replace secret "****" 

    let exec arguments = az arguments |> ignore 
