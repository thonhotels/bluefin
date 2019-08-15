namespace Bluefin.Kubernetes
open Bluefin.Core

open Fake.Core

module Core =
    // let kubectl arguments =
    //     execProcessString "kubectl" (argsToArray arguments) (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = "" }) 

    let kubectl arguments =
        execProcessString "kubectl" arguments (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = "" })

    let kubectlWorkingDir arguments workingDirectory =
        execProcessString "kubectl" (argsToArray arguments) (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = workingDirectory })

    let kubectlRedact arguments redact =
        execProcessString "kubectl" arguments (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = ""; RedactTrace = redact }) 

    let redactValue cmd secret = 
        cmd 
        |> String.replace secret "****" 

    let exec arguments = kubectl arguments |> ignore 

    let kubectlResult arguments = 
        let r = execProcess "kubectl" (argsToArray arguments) (fun o -> { o with DisplayName = "Kubectl"; WorkingDirectory = "" }) id

        { r with Result = { r.Result with Output = r.Result.Output.Trim()}}
