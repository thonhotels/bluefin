namespace Bluefin.Role

open Bluefin.Core

module Assignment =
    let private exists assigneeObjectId role scope = 
        let result = azResult (sprintf "role assignment list --assignee %s --role %s --scope %s" assigneeObjectId role scope)
        
        match result.Result.Error, result.ExitCode, result.Result.Output with
            | (_, 0, x) when (x = "[]") ->
                false
            | _, 0, x ->
                true
            | error, exitCode, _ ->
                failwithf "Failed to list role assignement. ExitCode %d. Message: %s" exitCode error

    let create assigneeObjectId role scope =
    
        if exists assigneeObjectId role scope then
            ""
        else
            let result = azResult (sprintf "role assignment create --assignee-object-id %s --role %s --scope %s" assigneeObjectId role scope)
    
            match result.Result.Error, result.ExitCode with
                | _, 0 ->                
                    result.Result.Output.Trim().Trim('"')            
                | error, exitCode ->
                    failwithf "Possibly transient error. ExitCode %d. Message: %s" exitCode error
    
