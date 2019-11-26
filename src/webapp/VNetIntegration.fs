namespace Bluefin.Webapp

open Bluefin.Core
open Bluefin.Common
open Bluefin.Http
open Newtonsoft.Json
open System.Net

module VNetIntegration =
    type CreateVNetConnectionProperties = {
       subnetResourceId: string
       swiftSupported: string
    }
    type CreateVNetConnection = {
        properties: CreateVNetConnectionProperties
        id: string
        location: string
    }
    let buildRequest rg siteName location subnetResourceId =
        let id = 
            sprintf "/subscriptions/%s/resourceGroups/%s/providers/Microsoft.Web/sites/%s/config/virtualNetwork" subscriptionId rg siteName
        {id = id;location=location;properties={subnetResourceId=subnetResourceId;swiftSupported="true"}}

    type Properties = {
        subnetResourceId: string
        swiftSupported: bool
    }

    type VNetConnection = {
        [<JsonProperty("type")>]
        _type: string
        properties: Properties
        id: string
        name: string
        location: string
    }

    let buildSubnetId vnetRg vnetName subnetName =
        sprintf "/subscriptions/%s/resourceGroups/%s/providers/Microsoft.Network/virtualNetworks/%s/subnets/%s" subscriptionId vnetRg vnetName subnetName

    let buildUrl rg siteName =
        sprintf "https://management.azure.com/subscriptions/%s/resourceGroups/%s/providers/Microsoft.Web/sites/%s/networkconfig/virtualNetwork?api-version=2018-11-01" subscriptionId rg siteName

    let get rg siteName =
        let url = buildUrl rg siteName
        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = get<VNetConnection> Management url (Some accessTokenResult.accessToken) 
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    value
               |(statusCode, value) -> failwithf "Could not get vnet integration. Status code is %A. Content: %A" statusCode value

    let add rg siteName vnetRg vnetName subnetName =
        let subnetId = buildSubnetId vnetRg vnetName subnetName

        let url = buildUrl rg siteName
        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put Management url (Some accessTokenResult.accessToken) (Some (box <| buildRequest rg siteName defaultLocation subnetId))
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    printfn "Updated vnet integration"
                    JsonConvert.DeserializeObject<VNetConnection> value
               |(HttpStatusCode.Created, value) ->
                    printfn "Added vnet integration"
                    JsonConvert.DeserializeObject<VNetConnection> value
               |(statusCode, value) -> failwithf "Could not create vnet integration. Status code is %A. Content: %s" statusCode value
