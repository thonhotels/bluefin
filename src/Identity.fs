namespace Bluefin

open System.Net
open Newtonsoft.Json
open Bluefin.Core
open Bluefin.Http

module Identity =
    type LocationType = {
        location: string
    }

    type Tag = {
        key: string
        value: string
    }
    type Properties = {
        clientId: string
        clientSecretUrl: string
        principalId: string
        tenantId: string
    }
    type ManagedIdentity = {
        id: string
        location: string
        name: string
        properties: Properties
        tags: System.Collections.Generic.Dictionary<string,string>
        [<JsonProperty("type")>]
        _type: string
    }

    let create rg name =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.ManagedIdentity/userAssignedIdentities/%s?api-version=2015-08-31-preview" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put Management url (Some accessTokenResult.accessToken) (Some (box {location = defaultLocation}))
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    printfn "Updated managed identity (user assigned)"
                    JsonConvert.DeserializeObject<ManagedIdentity> value
               |(HttpStatusCode.Created, value) ->
                    printfn "Created managed identity (user assigned)"
                    JsonConvert.DeserializeObject<ManagedIdentity> value
               |(statusCode, value) -> failwithf "Could not create managed identity (user assigned). Status code is %A. Content: %s" statusCode value

    let get rg name =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.ManagedIdentity/userAssignedIdentities/%s?api-version=2018-11-30" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = get<ManagedIdentity> Management url (Some accessTokenResult.accessToken)
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    value
               |(statusCode, value) -> failwithf "Could not get managed identity (user assigned). Status code is %A. Content: %A" statusCode value

    let resourceId rg name = 
        (Group.rgId rg) + (sprintf "/providers/Microsoft.ManagedIdentity/userAssignedIdentities/%s" name)

    let ensureServicePrincipalAvailable principalId =
        let mutable counter = 0
        System.Threading.Thread.Sleep 10000
        while (not (Ad.ServicePrincipal.exist principalId)) do
            counter <- counter + 1
            printfn "not yet found service principal"
            if counter = 10 then failwithf "serviceprincipal %s was not found after %d tries" principalId counter
            System.Threading.Thread.Sleep 10000
        printfn "service principal %s found" principalId
        ()