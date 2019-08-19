namespace Bluefin

open Fake.Core
open System.Text.RegularExpressions
open System.Net.Http
open Newtonsoft.Json

module Core =
    [<NoComparison>]
    [<NoEquality>]
    type ExecProcessOptions = {
        DisplayName: string
        WorkingDirectory: string
        RedactTrace: string -> string
    }

    let defaultOptions = { DisplayName = ""; WorkingDirectory = ""; RedactTrace = id }

    let internal execProcess name arguments createOptions resultFn =

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

    let internal execProcessString name arguments createOptions =
        execProcess name arguments createOptions (fun res -> 
                        if res.ExitCode <> 0 then failwithf "Step failed: %O" res.Result.Error
                        res.Result.Output.Trim().Trim('"'))

    let argsToArray args = // Split a command on whitespace, ignoring quoted sections
          let regex = Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)", RegexOptions.Multiline)
          regex.Split(args)
          |> Array.filter(String.isNullOrWhiteSpace >> not)

    let az arguments =
        execProcessString "az" (argsToArray arguments) (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = "" }) 

    let azArr arguments =
        execProcessString "az" arguments (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = "" })

    let azWorkingDir arguments workingDirectory =
        execProcessString "az" (argsToArray arguments) (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = workingDirectory })

    let azWorkingDirArr arguments workingDirectory =
        execProcessString "az" arguments (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = workingDirectory })

    let azRedact arguments redactedArgs =
        execProcessString "az" (argsToArray arguments) (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = ""; RedactTrace = fun _ -> redactedArgs }) 

    let redactValue cmd secret = 
        cmd 
        |> String.replace secret "****" 

    let exec arguments = az arguments |> ignore 

    let azResult arguments = 
        let r = execProcess "az" (argsToArray arguments) (fun o -> { o with DisplayName = "Azure CLI"; WorkingDirectory = "" }) id

        { r with Result = { r.Result with Output = r.Result.Output.Trim()}}

    let mutable tennantId = ""
    let mutable subscriptionId = ""
    let mutable private httpClient = None
    let mutable private graphClient = None

    type AccessTokenResult = {
        accessToken: string
        expiresOn: string
        subscription: string
        tenant:string
        tokenType:string
    }

    let private tokens = new System.Collections.Generic.Dictionary<_, _>()
    
    let private getAccessTokenEx resource = 
        let accessTokenResult = az (sprintf "account get-access-token -o json --resource %s" resource)
        let result = JsonConvert.DeserializeObject<AccessTokenResult>(accessTokenResult)
        tokens.Add(resource,result)
        result

    let getAccessToken resource =
        match tokens.TryGetValue resource with
        | true, token -> token
        | _ -> getAccessTokenEx resource 

    let ManagementHttpClient () =
        match httpClient with
        |Some x -> x
        |None -> failwith "Call init to initialize bluefin"
    
    let GraphClient () =
        match graphClient with
        |Some x -> x
        |None -> failwith "Call init to initialize bluefin"

    let init tid sid location= 
        tennantId <- tid   
        subscriptionId <- sid 
        let url = sprintf "https://management.azure.com/subscriptions/%s/" sid

        httpClient <- Some (new HttpClient ())
        httpClient.Value.BaseAddress <- (System.Uri url) 
        httpClient.Value.DefaultRequestHeaders.Clear ()
        httpClient.Value.DefaultRequestHeaders.Add ("location", [|location|]) 

        let graphUrl = sprintf "https://graph.windows.net/%s/" tid
        graphClient <- Some (new HttpClient ())
        graphClient.Value.BaseAddress <- (System.Uri graphUrl) 
        

