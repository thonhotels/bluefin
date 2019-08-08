namespace Bluefin.Storage

open Bluefin.Core
open Bluefin.Http
open System.Net
open Newtonsoft.Json


module Account =

    type Kind = BlobStorage | BlockBlobStorage | FileStorage | Storage | StorageV2 

    type SkuName =  Premium_LRS | Premium_ZRS | Standard_GRS | Standard_LRS | Standard_RAGRS | Standard_ZRS
    type SkuTier = Premium | Standard
    type SkuCapability = {
        name: string
        value: string
    }
    type ReasonCode = NotAvailableForSubscription | QuotaId
    type Restriction = {
       reasonCode: ReasonCode
       [<JsonPropertyAttribute(PropertyName = "type")>]
       _type: string
       values: seq<string> 
    }
    type Sku = {
        capabilities: seq<SkuCapability> option
        kind: Kind option
        locations: seq<string> option
        name: SkuName
        resourceType: string option
        restrictions: seq<Restriction> option
        tier: SkuTier option
    }
    type IdentityType = SystemAssigned
    type Identity = {
        principalId: string
        tenantId: string
        [<JsonPropertyAttribute(PropertyName = "type")>]
        _type: IdentityType
    }
    type AccessTier = Cool | Hot
    type CustomDomain = {
        name: string
        useSubDomainName: bool
    }
    type KeySource = Microsoft_Storage | Microsoft_Keyvault
    type KeyVaultProperties = {
        keyname: string
        keyvaulturi: string
        keyversion: string
    }
    type EncryptionService = {
        enabled: bool
        lastEnabledtime: string
    }
    type EncryptionServices = {
        blob: EncryptionService
        file: EncryptionService
        queue: EncryptionService
        table: EncryptionService
    }
    type Encryption = {
        keySource: KeySource
        keyvaultProperties: KeyVaultProperties
        services: EncryptionServices
    }
    type Bypass = {
        AzureServices: bool
        Logging: bool
        Metrics: bool
        None: bool
    }
    type DefaultAction = Allow | Deny
    type Action = Allow
    type IPRule = {
        action: Action
        value: string
    }
    type VirtualNetworkRule = {
        action: Action
        id: string
    }
    type NetworkRuleSet = {
        bypass: Bypass
        defaultAction: DefaultAction
        ipRules: seq<IPRule>
        virtualNetworkRules: seq<VirtualNetworkRule>
    }
    type StorageAccount = {
        identity: Identity option
        sku: Sku
        kind: Kind
        location: string
        accessTier: AccessTier option
        azureFilesAadIntegration: bool
        customDomain: CustomDomain option
        encryption: Encryption option
        isHnsEnabled: bool
        networkAcls: NetworkRuleSet option
        supportHttpsTrafficOnly: bool
        tags: Map<string, string>
    }
    let defaultSku = {
            name = Standard_GRS
            tier = None
            capabilities = None
            kind = None
            locations = None
            resourceType = None
            restrictions = None
        }

    let defaultAccount = {
        identity = None
        sku = defaultSku
        kind = Kind.BlobStorage
        location = ""
        accessTier = None
        azureFilesAadIntegration = false
        customDomain = None
        encryption = None
        isHnsEnabled = false
        networkAcls = None
        supportHttpsTrafficOnly = true
        tags = Map.empty
    }

    type Properties = {
        accessTier: AccessTier option
        azureFilesAadIntegration: bool
        customDomain: CustomDomain option
        encryption: Encryption option
        isHnsEnabled: bool
        networkAcls: NetworkRuleSet option
        supportHttpsTrafficOnly: bool
    }
    type AccountRequest = {
        identity: Identity option
        sku: Sku
        kind: Kind
        location: string
        properties: Properties
        
        tags: Map<string,string>
    }

    let create rg name (storageAccount:StorageAccount) =
        let toArgs (s:StorageAccount) = 
            Some (box {
                identity = s.identity
                sku = s.sku
                kind = s.kind
                location = s.location
                tags = s.tags
                properties = {
                    accessTier = s.accessTier
                    azureFilesAadIntegration = s.azureFilesAadIntegration
                    customDomain = s.customDomain
                    encryption = s.encryption
                    isHnsEnabled = s.isHnsEnabled
                    networkAcls = s.networkAcls
                    supportHttpsTrafficOnly = s.supportHttpsTrafficOnly
                }
            })
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.Storage/storageAccounts/%s?api-version=2018-02-01" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put Management url (Some accessTokenResult.accessToken) (toArgs storageAccount)
        match (result) with
               |(HttpStatusCode.OK, value) | (HttpStatusCode.Accepted, value) -> printfn "Updated storage account"
               |(HttpStatusCode.Created, value) -> printfn "Created storage account"
               |(statusCode, value) -> failwithf "Could not create storage account. Status code is %A. Content: %s" statusCode value

        ()