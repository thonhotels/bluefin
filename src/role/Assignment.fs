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

    let private exists assigneeObjectId role scope = 
        let result = azResult (sprintf "role assignment list --assignee %s --role %s --scope %s" assigneeObjectId role scope)
        
        match result.Result.Error, result.ExitCode, result.Result.Output with
            | (_, 0, x) when (x = "[]") ->
                false
            | _, 0, x ->
                true
            | error, exitCode, _ ->
                failwithf "Failed to list role assignement. ExitCode %d. Message: %s" exitCode error

    let create assigneeObjectId role s =
        let buildScope = 
            sprintf "/subscriptions/%s/resourceGroups/%s/providers/%s/%s/%s" s.subscription s.resourceGroup s.provider s.key s.value
    
        if exists assigneeObjectId role buildScope then
            ""
        else
            let result = azResult (sprintf "role assignment create --assignee-object-id %s --role %s --scope %s" assigneeObjectId role buildScope)
    
            match result.Result.Error, result.ExitCode with
                | _, 0 ->                
                    result.Result.Output.Trim().Trim('"')            
                | error, exitCode ->
                    failwithf "Possibly transient error. ExitCode %d. Message: %s" exitCode error
    
