namespace Bluefin.Servicebus

open Bluefin.Core
open Bluefin.Common

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
            if Seq.isEmpty sb.tags then ""
            else sprintf "--tags %s" (tagsToString sb.tags)
                         
        az (sprintf "servicebus namespace create -g %s -n %s --sku %s %s" rg name (sb.sku.ToString()) tagsArg) 

    let resourceId rg name =
        Common.resourceId rg "namespaces" name        