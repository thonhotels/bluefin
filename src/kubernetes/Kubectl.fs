namespace Bluefin.Kubernetes

open Fake.Core
open Bluefin.Kubernetes.Core
open Newtonsoft.Json
open Fake.IO

module Kubectl =
    let apply ymlFilePath =
        kubectl [|"apply"; "-f"; ymlFilePath|] |> ignore

    let applyYmls folder files =
        files
        |> Seq.map (fun f -> sprintf "%s/%s" folder f)
        |> Seq.iter apply

    let mergeYml folder env applyFn f =
        let filename = sprintf "%s/%s" folder f
        let readParams env = 
            let paramsFile = String.replace ".yml" (sprintf ".%s.json" env) filename
            if (File.exists paramsFile) then
                JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(File.readAsString paramsFile)
                |> Seq.map (fun i -> i.Key,i.Value)
            else
                Seq.empty

        let replaceContent name content = 
            printfn "%s" name
            File.replaceContent name content

        let replaceParamsInFile filename =
            let modifiedFile = String.replace ".yml" "-deploy.yml" filename              
            readParams env
            |> Seq.fold (fun state (key,value) -> String.replace ("___" + key + "___") value state) (File.readAsString filename)
            |> replaceContent modifiedFile
            modifiedFile
        
        applyFn <| replaceParamsInFile filename

    let mergeYmls folder env files =
        let mergeYmlFolder = mergeYml folder env apply
        files
        |> Seq.iter mergeYmlFolder
    
    let setContextWithNamespace currentContext k8sNameSpace =
        Trace.trace (sprintf "Set context: '%s'. Set namespace: '%s'." currentContext k8sNameSpace)
        kubectl ["config"; "set-context"; currentContext; "--namespace"; k8sNameSpace] |> ignore
        kubectl ["config"; "use-context"; currentContext] |> ignore

    let createSecrets storeName (secrets: List<string * string>) =
        let redactSecrets args =
              args 
              |> String.splitStr "--from-literal="     
              |> List.map (fun s ->
                            if not <| s.Contains "create secret" then
                               sprintf "%s=***** " (s.Split '=' |> Array.head)
                            else
                              s
                          )        
              |> String.concat "--from-literal="

        try
            kubectl ["delete"; "secret"; storeName]
            |> ignore
        with
            | _ -> ()

        kubectlRedact (["create"; "secret"; "generic"; storeName] @
                (secrets |> List.map (fun (key, value) -> sprintf "--from-literal=%s=%s" key value)))
                redactSecrets