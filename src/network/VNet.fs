namespace Bluefin.Network

open Bluefin.Core

module VNet =
    let private showVNet rg name =
        azResult (sprintf "network vnet show -g %s -n %s" rg name)

    let private showSubnet rg name vnetName =
        azResult (sprintf "network vnet subnet show -g %s -n %s --vnet-name %s" rg name vnetName)

    let exist (x:Fake.Core.ProcessResult<Fake.Core.ProcessOutput>) =         
        match x.Result.Error, x.ExitCode with
        | _, 0 -> true
        | e, 3 -> 
            printf "%s" e
            false
        | error, exitCode ->
            printf "Possibly transient error. Vnet may exist. ExitCode %d. Message: %s" exitCode error
            true

    let updateVNet rg name addressPrefixes =
        az (sprintf "network vnet update -g %s -n %s --address-prefixes %s" rg name addressPrefixes)

    let createVNet rg name addressPrefixes =
        let vnetExist =
            let x = showVNet rg name
            exist x

        if not vnetExist then
            az (sprintf "network vnet create -g %s -n %s --address-prefixes %s" rg name addressPrefixes)
        else
            updateVNet rg name addressPrefixes
 
    let updateSubnet rg name vnetName addressPrefixes =
        az (sprintf "network vnet subnet update -g %s -n %s --vnet-name %s --address-prefixes %s" rg name vnetName addressPrefixes)

    let createSubnet rg name vnetName addressPrefixes  = 
        az (sprintf "network vnet subnet create -g %s -n %s --vnet-name %s --address-prefixes %s" rg name vnetName addressPrefixes)
