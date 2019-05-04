namespace Bluefin

open Bluefin.Core

module Keyvault =
    let getSecret vaultName name =
        az (sprintf "keyvault secret show --vault-name %s --name %s --query value" vaultName name)