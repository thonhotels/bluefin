namespace Bluefin.Network

open Bluefin.Core

module VNet =
    let private show rg name =
        azResult (sprintf "network vnet show -g %s -n %s" rg name)

    let exist (x:Fake.Core.ProcessResult<Fake.Core.ProcessOutput>) =         
        match x.Result.Error, x.ExitCode with
        | _, 0 -> true
        | e, 3 -> 
            printf "%s" e
            false
        | error, exitCode ->
            printf "Possibly transient error. Vnet may exist. ExitCode %d. Message: %s" exitCode error
            true

    let update rg name addressPrefixes =
        az (sprintf "network vnet update -g %s -n %s --address-prefixes %s" rg name addressPrefixes)

    let create rg name addressPrefixes =
        let vnetExist =
            let x = show rg name
            exist x
        if not vnetExist then

            az (sprintf "network vnet create -g %s -n %s --address-prefixes %s" rg name addressPrefixes)
        else
            update rg name addressPrefixes

    let resourceId rg name =
        Common.resourceId rg "virtualNetworks" name   