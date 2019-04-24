namespace Bluefin.Webapp

open Bluefin.Core
open Newtonsoft.Json

module Identity =
    // type IdentityType = SystemAssigned | UserAssigned

    [<NoComparison>]
    type Identity = {
        identityIds: seq<string>
        principalId: string
        tenantId: string
        [<JsonProperty("type")>]
        _type: string
    }

    let assign rg name slot =
        let slotArg = 
            match slot with
            | Some s -> " -s " + s
            | None  -> ""

        let result = az ((sprintf "webapp identity assign -g %s -n %s" rg name) + slotArg)
        JsonConvert.DeserializeObject<Identity>(result)

    
