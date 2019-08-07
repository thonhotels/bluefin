namespace Bluefin.Network

open Bluefin.Core

module Subnet =
    let private show rg name vnetName =
        azResult (sprintf "network vnet subnet show -g %s -n %s --vnet-name %s" rg name vnetName)

    let update rg name vnetName addressPrefixes =
        az (sprintf "network vnet subnet update -g %s -n %s --vnet-name %s --address-prefixes %s" rg name vnetName addressPrefixes)

    let create rg name vnetName addressPrefixes  = 
        az (sprintf "network vnet subnet create -g %s -n %s --vnet-name %s --address-prefixes %s" rg name vnetName addressPrefixes)
 
    let resourceId rg vnetName name =
        Common.resourceId rg "virtualNetworks" vnetName + (sprintf "/subnets/%s" name)      