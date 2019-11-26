namespace Bluefin.Network

open Bluefin.Core

module Subnet =
    let private show rg name vnetName =
        azResult (sprintf "network vnet subnet show -g %s -n %s --vnet-name %s" rg name vnetName)

    let update rg name vnetName addressPrefixes =
        azArr ["network";"vnet";"subnet";"update";"-g";rg;"-n";name;"--vnet-name";vnetName;"--address-prefixes";addressPrefixes]

    let create rg name vnetName addressPrefixes  = 
        azArr ["network";"vnet";"subnet";"create";"-g";rg;"-n";name;"--vnet-name";vnetName;"--address-prefixes";addressPrefixes]

    let createOrUpdate rg name vnetName addressPrefixes =
        let subnet = show rg name vnetName
        match subnet with
        | Some _ -> update rg name vnetName addressPrefixes
        | None -> create rg name vnetName addressPrefixes
 
    let resourceId rg vnetName name =
        Common.resourceId rg "virtualNetworks" vnetName + (sprintf "/subnets/%s" name)