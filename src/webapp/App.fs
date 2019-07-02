namespace Bluefin.Webapp

open Bluefin.Core
open Config
open Bluefin.Http
open System.Net

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
        ipSecurityRestrictions: seq<IpSecurityRestriction>
    }

    let defaultWebApp = {
        resourceGroup = ""
        name = ""
        plan = ""
        planResourceGroup = None
        tags = [||]
        alwaysOn = true
        ipSecurityRestrictions = [||]
    }

    type SiteProperties = {
        httpsOnly: bool
        outboundIpAddresses: string
        serverFarmId: string
    }

    type Site = {
        id: string
        kind: string
        location: string
        name: string
        properties: SiteProperties
    }

    let get rg name =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.Web/sites/%s?api-version=2016-08-01" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = get<Site> url (Some accessTokenResult.accessToken)
        match (result) with
               |(HttpStatusCode.OK, site) -> Some site
               |(HttpStatusCode.NotFound, _) -> None
               |(statusCode, value) -> failwithf "Could not get web app. Status code is %A. Content: %A" statusCode value

               
    let private createWebApp rg plan planResourceGroup name tags =
        
        let siteCompatible site =
            let planId =
                let pRg = match planResourceGroup with 
                            | Some x -> x
                            | None -> rg
                sprintf "%s/providers/Microsoft.Web/serverfarms/%s" pRg plan
            site.properties.serverFarmId = planId


        let tagToString s =
            if System.String.IsNullOrEmpty s.value then s.key
            else sprintf "%s=%s" s.key s.value
        
        let planIdOrName name rg =
            match rg with
            | Some x -> sprintf "%s/providers/Microsoft.Web/serverfarms/%s" x name
            | None -> name

        let tagArg =
            if Seq.isEmpty tags then ""
            else "--tags " + (tags |> Seq.map tagToString |> String.concat " ")

        let existing = get rg name
        match existing with
        | Some site -> if (not (siteCompatible site)) then failwithf "Cant create web app. Site already exist with incompatible settings"
        | None -> az (sprintf "webapp create -g %s -p %s -n %s %s" rg (planIdOrName plan planResourceGroup) name tagArg) |> ignore
        

    let create a =
        createWebApp a.resourceGroup a.plan a.planResourceGroup a.name a.tags
        az (sprintf "webapp update -g %s -n %s --https-only true" a.resourceGroup a.name) |> ignore
        az (sprintf "webapp config set -g %s -n %s --always-on %b --remote-debugging-enabled false" a.resourceGroup a.name a.alwaysOn) |> ignore    
        if not (Seq.isEmpty a.ipSecurityRestrictions) then 
            addIpSecurityRestriction { 
                resourceGroup = a.resourceGroup
                appName = a.name
                ipSecurityRestrictions = a.ipSecurityRestrictions
            } |> ignore
        else ()