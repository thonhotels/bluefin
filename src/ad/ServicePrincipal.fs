namespace Bluefin.Ad

open Bluefin.Core
open System
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