namespace Bluefin

open Bluefin.Core

module Keyvault =
    let getSecret vaultName name =
        az (sprintf "keyvault secret show --vault-name %s --name %s --query value" vaultName name)

    let setSecret vaultName name value = 
        azRedact 
            (sprintf "keyvault secret set --vault-name %s --name %s --value %s" vaultName name value)
            (sprintf "keyvault secret set --vault-name %s --name %s --value %s" vaultName name "****")
            |> ignore    
