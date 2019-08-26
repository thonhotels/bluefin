namespace Bluefin

open System.Net
open Newtonsoft.Json
open Bluefin.Core
open Bluefin.Http

module Identity =
    type LocationType = {
        location: string
    }
    let create rg name =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.ManagedIdentity/userAssignedIdentities/%s?api-version=2015-08-31-preview" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put Management url (Some accessTokenResult.accessToken) (Some (box {location = defaultLocation}))
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    printfn "Updated managed identity (user assigned)"
                    value
               |(HttpStatusCode.Created, value) ->
                    printfn "Created managed identity (user assigned)"
                    value
               |(statusCode, value) -> failwithf "Could not create managed identity (user assigned). Status code is %A. Content: %s" statusCode value

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
    type IdentityResponse = {
        id: string
        location: string
        name: string
        properties: Properties
        tags: System.Collections.Generic.Dictionary<string,string>
        [<JsonProperty("type")>]
        _type: string
    }

    let get rg name =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.ManagedIdentity/userAssignedIdentities/%s?api-version=2018-11-30" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = get<IdentityResponse> Management url (Some accessTokenResult.accessToken)
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    value
               |(statusCode, value) -> failwithf "Could not get managed identity (user assigned). Status code is %A. Content: %A" statusCode value

    let resourceId rg name = 
        (Group.rgId rg) + (sprintf "/providers/Microsoft.ManagedIdentity/userAssignedIdentities/%s" name)    