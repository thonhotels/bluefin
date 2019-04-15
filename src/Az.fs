namespace Bluefin

open Bluefin.Core

module Az =

    let login tenantId servicePrincipalId password = 
        sprintf "login --service-principal --username %s  --password %s --tenant %s" servicePrincipalId password tenantId
        |> fun cmd ->     
            azRedact cmd (redactValue cmd password)

    let getRedisKey name rg =
        az (sprintf "redis list-keys --name %s --resource-group %s --query primaryKey" name rg)

    let getSecret vaultName secretName = 
        az (sprintf "keyvault secret show --vault-name %s --name %s --query value" vaultName secretName)

    let setSecret vaultName secretName value = 
        az (sprintf "keyvault secret set --vault-name %s --name %s --value %s" vaultName secretName value) |> ignore    
    
    let withFirewallOpening ip rg dbServer fn =
            let ruleName = sprintf "temporary-rule-%O" (System.Guid.NewGuid())
            az (sprintf "sql server firewall-rule create -g %s --server %s --name %s --start-ip-address %s --end-ip-address %s" rg dbServer ruleName ip ip) |> ignore
    
            try 
                fn() 

            finally
                az (sprintf "sql server firewall-rule delete -g %s --server %s --name %s" rg dbServer ruleName)
                |> ignore