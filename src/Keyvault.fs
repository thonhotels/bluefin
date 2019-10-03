namespace Bluefin

open Bluefin.Core
open Bluefin.Http
open System.Net
open Microsoft.FSharp.Core
open System

module Keyvault =
    let getSecret vaultName name =
        az (sprintf "keyvault secret show --vault-name %s --name %s --query value" vaultName name)

    let setSecret vaultName name value = 
        azRedact ["keyvault"
                  "secret"
                  "set"
                  "--vault-name"
                  vaultName
                  "--name"
                  name
                  "--value"
                  value]
            (sprintf "keyvault secret set --vault-name %s --name %s --value ****" 
                vaultName 
                name)
        |> ignore

    // type CertificatePermission = encrypt | decrypt | wrapKey | unwrapKey | sign |
    //                                 verify | get | list | create | update | import |
    //                                 delete | backup | restore | recover | purge
    type Permissions = {
        certificates: string[]
        keys: string[]
        secrets: string[]
        storage: string[]
    }

    let emptyPermissions = {
        certificates = Array.empty
        keys = Array.empty
        secrets = Array.empty
        storage = Array.empty
    }
    type AccessPolicyEntry = {
       applicationId: string option
       objectId: string
       permissions: Permissions
       tenantId: string 
    }

    //type NetworkRuleBypassOptions = AzureServices | None

    type NetworkRuleAction = Allow | Deny

    type IPRule = {
        value: string
    }

    type VirtualNetworkRule = {
        id: string
    }

    type NetworkRuleSet = {
        bypass: string // AzureServices | None
        defaultAction: NetworkRuleAction
        ipRules: IPRule[]
        virtualNetworkRules: VirtualNetworkRule[]
    }
    
    type SkuFamily = A

    
    type Sku = {
        family: SkuFamily
        name: string //premium | standard
    }

    type VaultProperties = {
        accessPolicies: AccessPolicyEntry[] option
        createMode: string option//default | recover
        enablePurgeProtection: bool option
        enableSoftDelete: bool option
        enabledForDeployment: bool option
        enabledForDiskEncryption: bool option
        enabledForTemplateDeployment: bool option
        networkAcls: NetworkRuleSet option
        sku: Sku
        tenantId: string
        vaultUri: string option
    }

    type KeyvaultArgs = {
        location: string
        properties: VaultProperties
        tags: Map<string,string>
    }

    let defaultProperties =  {
        accessPolicies = None
        createMode = None
        enablePurgeProtection = None
        enableSoftDelete = None
        enabledForDeployment = Some false
        enabledForDiskEncryption = Some false
        enabledForTemplateDeployment = Some false
        networkAcls = None
        sku = {
            family = SkuFamily.A
            name = "standard"
        }
        tenantId = tenantId
        vaultUri = None
    }

    let defaultArgs = {
        location = defaultLocation
        properties = defaultProperties
        tags = Map.empty
    }

    let create rg name (args:KeyvaultArgs)=

        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.KeyVault/vaults/%s?api-version=2018-02-14" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put Management url (Some accessTokenResult.accessToken) (Some (box args))
        match (result) with
               |(HttpStatusCode.OK, value) | (HttpStatusCode.Accepted, value) -> printfn "Updated keyvault"
               |(HttpStatusCode.Created, value) -> printfn "Created keyvault"
               |(statusCode, value) -> failwithf "Could not create keyvault. Status code is %A. Content: %s" statusCode value

        ()

    let getSecretPermission objectId secrets = 
      { applicationId = None
        objectId = objectId
        permissions = { emptyPermissions  with 
                          secrets = secrets
                      }
        tenantId = tenantId}
    