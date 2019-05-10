namespace Bluefin

open Bluefin.Core
open Newtonsoft.Json

module Az =
    let login tenantId servicePrincipalId password = 
        sprintf "login --service-principal --username %s  --password %s --tenant %s" servicePrincipalId password tenantId
        |> fun cmd ->     
            azRedact cmd (redactValue cmd password)

    let getRedisKey rg name =
        az (sprintf "redis list-keys --resource-group %s --name %s --query primaryKey" rg name)
    
    let withFirewallOpening ip rg dbServer fn =
            let ruleName = sprintf "temporary-rule-%O" (System.Guid.NewGuid())
            az (sprintf "sql server firewall-rule create -g %s --server %s --name %s --start-ip-address %s --end-ip-address %s" rg dbServer ruleName ip ip) |> ignore
    
            try 
                fn() 

            finally
                az (sprintf "sql server firewall-rule delete -g %s --server %s --name %s" rg dbServer ruleName)
                |> ignore