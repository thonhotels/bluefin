namespace Bluefin

open Bluefin.Core
open Newtonsoft.Json

module Group =
    type ResourceGroup = {
        id: string
        location: string
        managedBy: string
        name: string
        tags: seq<string>
    }

    let createResourceGroup name location =
        az (sprintf "group create --name %s --location %s" name location)

    let findResourceGroupByName name =
        let r = az (sprintf @"group list --query [?contains(name,'%s')]" name)
        JsonConvert.DeserializeObject<seq<ResourceGroup>> r

    let applyArmTemplate rg templatePath templateFile parameterFile = 
        azWorkingDir (sprintf "group deployment create -g %s --template-file %s --parameters %s" rg templateFile parameterFile) templatePath
            |> ignore