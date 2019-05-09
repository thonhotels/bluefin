namespace Bluefin.Webapp

open Bluefin.Core
open Config

module App =
    type Tag = {
        key: string
        value: string
    }
    
    type WebApp = { 
        resourceGroup: string   // Name of resource group
        name: string            // Name of the new web app
        plan: string            // Name or resource id of the app service plan
        planResourceGroup: string option // Set this if app service plan is not in the same resource group as the web app
                                // on the form: /subscriptions/<subscriptionId>/resourceGroups/<resourcegroup name>
        tags: seq<Tag>          // tags as key/value
        alwaysOn: bool          // Ensure web app gets loaded all the time, rather unloaded after been idle
        subscription: string
        ipSecurityRestrictions: seq<IpSecurityRestriction>
    }

    let defaultWebApp = {
        resourceGroup = ""
        name = ""
        plan = ""
        planResourceGroup = None
        tags = [||]
        alwaysOn = true
        subscription = ""
        ipSecurityRestrictions = [||]
    }

    let create a =
        let tagToString s =
            if System.String.IsNullOrEmpty s.value then s.key
            else sprintf "%s=%s" s.key s.value
        
        let planIdOrName name rg =
            match rg with
            | Some x -> sprintf "%s/providers/Microsoft.Web/serverfarms/%s" x name
            | None -> name

        let tagArg =
            if Seq.isEmpty a.tags then ""
            else "--tags " + (a.tags |> Seq.map tagToString |> String.concat " ")
        az (sprintf "webapp create -g %s -p %s -n %s %s" a.resourceGroup (planIdOrName a.plan a.planResourceGroup) a.name tagArg) |> ignore 
        az (sprintf "webapp update -g %s -n %s --https-only true" a.resourceGroup a.name) |> ignore
        az (sprintf "webapp config set -g %s -n %s --always-on %b --remote-debugging-enabled false" a.resourceGroup a.name a.alwaysOn) |> ignore    
        if not (Seq.isEmpty a.ipSecurityRestrictions) then 
            addIpSecurityRestriction { 
                subscription = a.subscription
                resourceGroup = a.resourceGroup
                appName = a.name
                ipSecurityRestrictions = a.ipSecurityRestrictions
            } |> ignore
        else ()