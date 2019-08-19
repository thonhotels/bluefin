
namespace Bluefin.Webapp

open Bluefin.Core

module Deployment =
    type ConnectionStringType = 
        | ApiHub | Custom | DocDb | EventHub | MySql | NotificationHub
        | PostgreSQL | RedisCache | SQLAzure | SQLServer | ServiceBus

    let setSettings rg webappName slot settings =
        let settingsArg =
            settings |>
            Seq.map (fun (key, value) -> sprintf "%s=%s" key value)            

        azArr <| Seq.append ["webapp";"config";"appsettings";"set";"-g";rg;"-n";webappName;"-s";slot;"--setting"] settingsArg

    let setConnectionStrings rg webappName slot (connectionString:ConnectionStringType*string) =
        let (cstype, cstring) = connectionString

        azArr ["webapp";"config";"connection-string";"set";"-g";rg;"-n";webappName;"-s";slot;"-t";cstype.ToString();"--setting";cstring] |> ignore
        ()

    let createSlot rg webappName slotName configurationSource =
        azArr ["webapp";"deployment";"slot";"create";"-g";rg;"-n";webappName;"-s";slotName;"--configuration-source";configurationSource]

    let createStagingSlot rg webappName settings connectionStrings =
        createSlot rg webappName "staging" webappName |> ignore
        setSettings rg webappName "staging" settings |> ignore
        Seq.iter (setConnectionStrings rg webappName "staging") connectionStrings |> ignore