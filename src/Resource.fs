namespace Bluefin

open Bluefin.Core

module Resource =
    let getInstrumentationKey rg name =    
        az (sprintf "resource show -g %s -n %s --resource-type microsoft.insights/components --query properties.InstrumentationKey" rg name)
