namespace Bluefin.Servicebus

open Bluefin.Core

module Namespace =
    type SkuType = Basic | Premium | Standard
    type ServicebusNamespace = {
        sku: SkuType // Allowed values: Basic, Premium, Standard.  Default: Standard.
        tags: seq<string*string>
    }
    
    let defaultServicebusNamespace = {
        sku = Standard
        tags = [||]
    }

    let create rg name sb =
        let tagsArg =
            let tag (key, value) = 
                sprintf "%s=%s" key value
            if Seq.isEmpty sb.tags then ""
            else sprintf "--tags %s" (sb.tags |> Seq.map tag |> String.concat " ")
                         
        az (sprintf "servicebus namespace create -g %s -n %s --sku %s %s" rg name (sb.sku.ToString()) tagsArg) 