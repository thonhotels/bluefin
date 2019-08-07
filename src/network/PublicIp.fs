namespace Bluefin.Network

open Bluefin.Core

module PublicIp =
    type AllocationMethod = Dynamic | Static
    type Sku = Basic | Standard

    let createAdvanced rg name (allocationMethod:AllocationMethod) dnsName (sku:Sku) =
        let dnsSegment = 
            match dnsName with
            | Some n -> sprintf "--dns-name %s" n
            | None -> ""

        az (sprintf "network public-ip create -g %s --n %s --allocation-method %s --sku %s %s" rg name (allocationMethod.ToString()) (sku.ToString()) dnsSegment)
        
    let createBasic rg name (allocationMethod:AllocationMethod) = 
        createAdvanced rg name allocationMethod None Sku.Basic

    let createStandard rg name = 
        createAdvanced rg name AllocationMethod.Static None Sku.Standard
   
    let resourceId rg name =
        Common.resourceId rg "publicIPAddresses" name        
