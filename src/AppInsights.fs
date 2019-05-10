namespace Bluefin

open Bluefin.Core

module AppInsights =
    type Applicationtype = MobileCenter | NodeJS | Java | Other | Web

    type AppInsightsOptions = {
        SamplingPercentage: float
        ApplicationType: Applicationtype
        Properties: seq<string * string>
    }

    let defaultOptions = { 
        SamplingPercentage = 100.0
        ApplicationType = Web
        Properties = [||]
    }

    let createAppInsights rg name location options =

        let typeToString t = 
            match t with
            | MobileCenter -> "MobileCenter"
            | NodeJS -> "Node.JS"
            | Other -> "other"
            | Java -> "java"
            | Web -> "web"

        let props =
            [|("SamplingPercentage", sprintf "%f" options.SamplingPercentage);("Application_Type", typeToString options.ApplicationType)|]
            |> Seq.append options.Properties
            |> Seq.map (fun (key, value) -> sprintf "\"%s\":\"%s\"" key value)
            |> String.concat ","
            |> sprintf "{%s}"

        az (sprintf "resource create -g %s --resource-type Microsoft.Insights/components -n %s -l %s --properties %s" rg name location props) |> ignore
    
    let setDataCap rg appInsightsName componentName dataVolumeCap =
        let options = sprintf "{ \"CurrentBillingFeatures\": \"Basic\", \"DataVolumeCap\": { \"Cap\": \"%f\" } }" dataVolumeCap
        azArr [|
                "resource"
                "create"
                "--id"
                (sprintf "/subscriptions/%s/resourceGroups/%s/providers/Microsoft.Insights/components/%s/CurrentBillingFeatures/%s" Core.subscriptionId rg appInsightsName componentName)
                "-p"
                options
              |] |> ignore