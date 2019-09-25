namespace Bluefin

open System
open System.Net
open Bluefin.Core
open Bluefin.Http
open Newtonsoft.Json

module AppConfig =
    
    type ConfigurationStoreKey = {
        id: string
        name: string
        value: string
        connectionString: string
        lastModified: string
        readOnly: bool
    }

    type ValueResult = {
        value: list<ConfigurationStoreKey>
    }

    let getConnectionStrings resourceGroup name =
        let accessTokenResult = getAccessToken "https://management.azure.com/"

        let url = sprintf "https://management.azure.com/subscriptions/%s/resourceGroups/%s/providers/Microsoft.AppConfiguration/configurationStores/%s/ListKeys?api-version=2019-02-01-preview" subscriptionId resourceGroup name
        let result = Bluefin.Http.post Management url (Some accessTokenResult.accessToken) None
        match (result) with
               |(Net.HttpStatusCode.OK, response) -> (JsonConvert.DeserializeObject<ValueResult> response).value
               |(statusCode, response) -> failwithf "Failed to get connectionStrings for %s %s. Status code is %A. Content: %A" resourceGroup name statusCode response

    let getFirstConnectionString resourceGroup name =
        List.head <| getConnectionStrings resourceGroup name
    
    type LocationType = {
        location: string
    }

    type Properties = {
        provisioningState: string
        creationDate: string
        endpoint: string
    }

    type ConfigurationStore = {
        [<JsonProperty("type")>]
        _type: string
        properties: Properties
        id: string
        name: string
        location: string
    }

    let create rg name =
        let url = sprintf "https://management.azure.com/subscriptions/%s/resourceGroups/%s/providers/Microsoft.AppConfiguration/configurationStores/%s?api-version=2019-02-01-preview" subscriptionId rg name
        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put Management url (Some accessTokenResult.accessToken) (Some (box {location = defaultLocation}))
        match (result) with
               |(HttpStatusCode.OK, value) -> 
                    printfn "Updated appconfig"
                    JsonConvert.DeserializeObject<ConfigurationStore> value
               |(HttpStatusCode.Created, value) ->
                    printfn "Created appconfig"
                    JsonConvert.DeserializeObject<ConfigurationStore> value
               |(statusCode, value) -> failwithf "Could not create appconfig %s %s. Status code is %A. Content: %s" rg name statusCode value


    