namespace Bluefin

open Bluefin.Core
open Bluefin.Http
open System.Net

module Identity =
    type LocationType = {
        location: string
    }
    let create rg name =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.ManagedIdentity/userAssignedIdentities/%s?api-version=2015-08-31-preview" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put url (Some accessTokenResult.accessToken) (Some (box {location = "westeurope"}))
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    printfn "Updated managed identity (user assigned)"
                    value
               |(HttpStatusCode.Created, value) ->
                    printfn "Created managed identity (user assigned)"
                    value
               |(statusCode, value) -> failwithf "Could not create managed identity (user assigned). Status code is %A. Content: %s" statusCode value

    let get rg name =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.ManagedIdentity/userAssignedIdentities/%s?api-version=2015-08-31-preview" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = get url (Some accessTokenResult.accessToken)
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    value
               |(statusCode, value) -> failwithf "Could not get managed identity (user assigned). Status code is %A. Content: %s" statusCode value
