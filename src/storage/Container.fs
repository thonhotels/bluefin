namespace Bluefin.Storage

open Bluefin.Core
open Bluefin.Http
open System.Net

module Container =
    type PublicAccess = Blob | Container | None
    type ContainerArgs = {
        metadata: Map<string,string> option
        publicAccess: PublicAccess option
    }
    let defaultArgs = {
        metadata = Option.None
        publicAccess = Option.None
    }

    let create rg accountName name c =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.Storage/storageAccounts/%s/blobServices/default/containers/%s?api-version=2018-11-01" rg accountName name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put url (Some accessTokenResult.accessToken) (Some (box c))
        match (result) with
               |(HttpStatusCode.OK, value) | (HttpStatusCode.Accepted, value) -> printfn "Updated container"
               |(HttpStatusCode.Created, value) -> printfn "Created container"
               |(statusCode, value) -> failwithf "Could not create container. Status code is %A. Content: %s" statusCode value

        ()