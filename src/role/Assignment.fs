namespace Bluefin.Role

open Bluefin.Core

module Assignment =
    type Scope = 
        { subscription: string
          resourceGroup: string
          provider: string
          key: string
          value: string
        }

    let create assigneeObjectId role s =
        let buildScope = 
            sprintf "/subscriptions/%s/resourceGroups/%s/providers/%s/%s/%s" s.subscription s.resourceGroup s.provider s.key s.value

        az (sprintf "role assignment create --assignee-object-id %s --role %s --scope %s" assigneeObjectId role buildScope)
    
