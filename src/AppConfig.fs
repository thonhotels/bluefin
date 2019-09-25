namespace Bluefin

open System
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
