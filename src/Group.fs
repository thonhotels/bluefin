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
        azArr ["group";"create";"--name"; name; "--location"; location]

    let findResourceGroupByName name =
        let r = azArr ["group"; "list"; "--query"; sprintf "[?contains(name,'%s')]" name]
        JsonConvert.DeserializeObject<seq<ResourceGroup>> r

    let applyArmTemplate rg templatePath templateFile parameterFile = 
        azWorkingDirArr ["deployment";"group";"create";"-g";rg;"--template-file";templateFile;"--parameters";parameterFile] templatePath
            |> ignore

    let rgId name =
        sprintf "/subscriptions/%s/resourceGroups/%s" subscriptionId name