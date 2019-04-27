
namespace Bluefin.Webapp

open Bluefin.Core

module Deployment =
    type ConnectionStringType = 
        | ApiHub | Custom | DocDb | EventHub | MySql | NotificationHub
        | PostgreSQL | RedisCache | SQLAzure | SQLServer | ServiceBus

    let setSettings rg webappName slot settings =
        let settingsArg =
            settings |>
            Seq.map (fun (key, value) -> sprintf "%s=%s" key value) |>
            String.concat " "

        az (sprintf "webapp config appsettings set -g %s -n %s -s %s --setting %s" rg webappName slot settingsArg) 

    let setConnectionStrings rg webappName slot (connectionString:ConnectionStringType*string) =
        let (cstype, cstring) = connectionString
        az (sprintf "webapp config connection-string set -g %s -n %s -s %s -t %s --setting %s" rg webappName slot (cstype.ToString()) cstring) |> ignore
        ()

    let createSlot rg webappName slotName configurationSource =
        az (sprintf "webapp deployment slot create -g %s -n %s -s %s --configuration-source %s" rg webappName slotName configurationSource) 

    let createStagingSlot rg webappName settings connectionStrings =
        createSlot rg webappName "staging" webappName |> ignore
        setSettings rg webappName "staging" settings |> ignore
        Seq.iter (setConnectionStrings rg webappName "staging") connectionStrings |> ignore