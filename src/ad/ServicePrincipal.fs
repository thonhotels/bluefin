namespace Bluefin.Ad

open System
open Bluefin.Core
open Bluefin.Http
open Newtonsoft.Json

module ServicePrincipal =
    type KeyCredential = {
        additionalProperties: string
        customKeyIdentifier: string
        endDate: string
        keyId: string
        startDate: string
        [<JsonProperty("type")>]
        _type: string
        usage: string
        value: string
    }

    type ServicePrincipal = {
        accountEnabled: bool
        appDisplayName: string
        appId: string
        appOwnerTenantId: string
        appRoleAssignmentRequired: bool
        applicationTemplateId: string
        deletionTimestamp: string
        displayName: string
        errorUrl: string
        homePage: string
        keyCredentials: KeyCredential[]
        logoutUrl: string
        notificationEmailAddresses: string[]        
        objectId: string
        objectType: string
        [<JsonProperty("odata.metadata")>]
        odata_metadata: string
        [<JsonProperty("odata.type")>]
        odata_type: string
        servicePrincipalNames: string[]
        servicePrincipalType: string        
    }
    
    let get id =
        let sp = az (sprintf "ad sp show --id %s" id)
        JsonConvert.DeserializeObject<ServicePrincipal> sp

    type ExistQuery = {
        objectIds: string[]
        includeDirectoryObjectReferences: bool 
    }

    type ExistResponse = {
        value: Object[]
    }
    let exist objId =
        let deserialize value = 
            JsonConvert.DeserializeObject<ExistResponse> value

        let objectFound obj =
            obj.value.Length > 0

        let accessTokenResult = getAccessToken "https://graph.windows.net/"

        let result = post Graph "getObjectsByObjectIds?api-version=1.6" (Some accessTokenResult.accessToken) (Some (box {objectIds = [|objId|];includeDirectoryObjectReferences=true}))
        match (result) with
               |(Net.HttpStatusCode.OK, value) -> 
                    objectFound (deserialize value)              
               |(statusCode, value) -> failwithf "Failed to get sp. Status code is %A. Content: %s" statusCode value 