namespace Bluefin.Webapp

open Bluefin.Core

module Identity =
    let assign rg name slot =
        let slotArg = 
            match slot with
            | Some s -> " -s " + s
            | None  -> ""

        az ((sprintf "webapp identity assign -g %s -n %s" rg name) + slotArg)
