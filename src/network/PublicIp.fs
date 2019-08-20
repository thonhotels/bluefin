namespace Bluefin.Network

open System.Net
open Bluefin.Core
open Bluefin.Http
open Newtonsoft.Json

module PublicIp =
    type AllocationMethod = Dynamic | Static
    type Sku = Basic | Standard

    let createAdvanced rg name (allocationMethod:AllocationMethod) dnsName (sku:Sku) =
        let dnsSegment = 
            match dnsName with
            | Some n -> ["--dns-name";n]
            | None -> []

        azArr <| ["network";"public-ip";"create";"-g";rg;"-n";name;"--allocation-method";allocationMethod.ToString();"--sku";sku.ToString()] @ dnsSegment
        
    let createBasic rg name (allocationMethod:AllocationMethod) = 
        createAdvanced rg name allocationMethod None Sku.Basic

    let createStandard rg name = 
        createAdvanced rg name AllocationMethod.Static None Sku.Standard
   
    let resourceId rg name =
        Common.resourceId rg "publicIPAddresses" name        

    type DnsSettings = {
        domainNameLabel: string
        fqdn: string
    }

    type SkuType = {
        name: Sku
        tier: string
    }
    type IPAddressVersion = IPv4 | IPv6
    type ProvisioningState =  Succeeded | Updating | Deleting | Failed
    type IpAddressProperties = {
        provisioningState: string
        resourceGuid: string
        ipAddress: string
        publicIPAddressVersion: string
        publicIPAllocationMethod: string        
        idleTimeoutInMinutes: int
        dnsSettings: DnsSettings
    }
    type IpAddressResponse = {
        name: string
        id: string
        etag: string
        location: string
        [<JsonProperty("type")>]
        _type: string
        properties: IpAddressProperties
        sku: SkuType
    }
    let get rg name =
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.Network/publicIPAddresses/%s?api-version=2017-04-01" rg name

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = get<IpAddressResponse> Management url (Some accessTokenResult.accessToken)
        match (result) with
               |(HttpStatusCode.OK, value) -> value
               |(statusCode, value) -> (failwithf "Could not get public ip address. Status code is %A. Content: %A" statusCode value)
