namespace Bluefin.Network

open Bluefin.Core

module Subnet =
    let private show rg name vnetName =
        azResult (sprintf "network vnet subnet show -g %s -n %s --vnet-name %s" rg name vnetName)

    let private subnetCreationArguments op rg name vnetName addressPrefixes =
        [
            "network";"vnet";"subnet";op
            "-g";rg;"-n";name;"--vnet-name";vnetName
            "--address-prefixes";addressPrefixes
        ]

    let update rg name vnetName addressPrefixes =
        azArr <| subnetCreationArguments "update" rg name vnetName addressPrefixes

    let create rg name vnetName addressPrefixes = 
        azArr <| subnetCreationArguments "create" rg name vnetName addressPrefixes

    let createOrUpdate op rg name vnetName addressPrefixes delegations =
        subnetCreationArguments op rg name vnetName addressPrefixes @ ["--delegations";delegations]
        |> azArr

    let createOrUpdateForWeb rg name vnetName addressPrefixes =
        let result = show rg name vnetName

        let op = match result.ExitCode with
                    | 0 -> "update"
                    | _ -> "create"
        createOrUpdate op rg name vnetName addressPrefixes "Microsoft.Web/serverfarms"

    let resourceId rg vnetName name =
        Common.resourceId rg "virtualNetworks" vnetName + (sprintf "/subnets/%s" name)