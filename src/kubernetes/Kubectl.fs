namespace Bluefin.Kubernetes

open Fake.Core
open Fake.Core.String.Operators
open Bluefin.Kubernetes.Core
open Newtonsoft.Json
open Fake.IO

module Kubectl =
    let apply ymlFilePath =
        kubectl [|"apply"; "-f"; ymlFilePath|] |> ignore

    let applyWithReplacements filename replacements =        
        let replaceParamsInFile filename =
            let modifiedFile = "\.y[a]?ml" >=> "-deploy.yaml" <| filename
            replacements
            |> Seq.fold (fun state (key,value) -> String.replace ("${" + key + "}") value state) (File.readAsString filename)
            |> File.replaceContent modifiedFile
            modifiedFile
        
        apply <| replaceParamsInFile filename 

    let createNamespace name =
        kubectl [|"create"; "namespace"; name|] |> ignore

    let label resourceType name kv =
        let (key, value) = kv
        let kvString = sprintf "%s=%s" key value
        kubectl [|"label"; resourceType; name; kvString|] |> ignore

    let applyYmls folder files =
        files
        |> Seq.map (fun f -> sprintf "%s/%s" folder f)
        |> Seq.iter apply

    let mergeYml folder applyFn paramsFn f :unit =
        let filename = sprintf "%s/%s" folder f
        
        let replaceContent name content = 
            File.replaceContent name content

        let replaceParamsInFile filename =
            let modifiedFile = "\.y[a]?ml" >=> "-deploy.yaml" <| filename
            paramsFn filename
            |> Seq.fold (fun state (key,value) -> String.replace ("${" + key + "}") value state) (File.readAsString filename)
            |> replaceContent modifiedFile
            modifiedFile
        
        applyFn <| replaceParamsInFile filename

    let mergeYmls folder env parameters files =
        let readParams env filename = 
            let paramsFile = "\.y[a]?ml" >=> (sprintf ".%s.json" env) <| filename
            if (File.exists paramsFile) then
                JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(File.readAsString paramsFile)
                |> Seq.map (fun i -> i.Key,i.Value)
                |> Seq.append parameters                
            else
                parameters

        let mergeYmlFolder fileName = mergeYml folder apply (readParams env) fileName
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