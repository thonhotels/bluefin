namespace Bluefin

open Bluefin.Core
open Bluefin.Common

module AppServicePlan =    

    type SkuType = B1 | B2 | B3 | D1 | F1 | FREE | P1V2 | P2V2 | P3V2 | PC2 | PC3 | PC4 | S1 | S2 | S3 | SHARED // Default is B1

    type AppServicePlanSettings = {        
        hyperv: bool                    // Host web app on Windows container
        isLinux: bool                   // Host web app on Linux worker
        location: string option         // Location
        numberOfWorkers: int            // Number of workers to be allocated. Default is 1
        sku: SkuType                    // The pricing tiers, e.g., F1(Free), D1(Shared), B1(Basic Small), B2(Basic Medium), B3(Basic Large), S1(Standard Small), P1V2(Premium V2 Small), PC2 (Premium Container Small), PC3 (Premium Container Medium), PC4 (Premium Container Large)
        subscription: string option     // Name or ID of subscription
        tags: seq<string*string>        // tags as key/value
    }

    let defaultAppServicePlanSettings = {
        hyperv = false
        isLinux = false
        location = None
        numberOfWorkers = 1
        sku = SkuType.B1
        subscription = None
        tags = [||]
    }

    let createPlan rg name settings =            
        let arr  = 
            ["appservice";"plan";"create";"-g";rg;"-n";name;                    
                    "--number-of-workers";settings.numberOfWorkers.ToString();
                    "--sku";settings.sku.ToString();
                    "--location";locationOrDefault settings.location;
            ] 
            @ if settings.hyperv then ["--hyper-v"] else [] 
            @ if settings.isLinux then ["--is-linux"] else []
            @ match settings.subscription with | Some subscription -> ["--subscription";subscription] | None -> []
            @ tagsToArgs settings.tags
        azArr arr