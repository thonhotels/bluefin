namespace Bluefin.Role

open Bluefin.Core
open System
open Bluefin.Http
open Newtonsoft.Json

module Assignment =
    type RoleAssignment = {
        id: string
        name: string
        principalId: string
        principalName: string
        roleDefinitionId: string
        roleDefinitionName: string
        scope: string
        [<JsonProperty("type")>]
        _type: string
    }

    let private get assigneeObjectId role scope =
        let result = azResult (sprintf "role assignment list --assignee %s --role %s --scope %s" assigneeObjectId role scope)
        
        match result.Result.Error, result.ExitCode, result.Result.Output with
            | (_, 0, x) when (x = "[]") ->
                None
            | _, 0, x ->
                Some (Seq.head <| JsonConvert.DeserializeObject<RoleAssignment[]> x)
            | error, exitCode, _ ->
                failwithf "Failed to list role assignement. ExitCode %d. Message: %s" exitCode error    

    let createAssignmentByAssigneeObjectId assigneeObjectId role scope principalType =
        azArr [|"role";"assignment";"create";"--role";role;"--scope"; scope;"--assignee-object-id";assigneeObjectId;"--assignee-principal-type";principalType|]

    type AssignRoleProperties = {
        roleDefinitionId: string
        principalId: string
    }
    type AssignRoleValues = {
        properties: AssignRoleProperties
    }
    
    type ErrorCode = {
        code: string
    }
    type BadRequestResponse = {
        error: ErrorCode
    }

    type BadRequests = PrincipalNotFound | Unknown

    type RoleAssignmentProperties = {
        roleDefinitionId: string
        principalId: string
        principalType: string
        scope: string
        createdOn: string
        updatedOn: string
        createdBy: string
        updatedBy: string
    }
    type CreateRoleAssigment = {
        id: string
        [<JsonProperty("type")>]
        _type: string
        name: string
        properties: RoleAssignmentProperties
    }

    type CreateAssignmentResult = 
    | RA of RoleAssignment
    | Code of BadRequests 

    let create assigneeObjectId role (scope:string) =
        let decodeBadRequest value =
            let response = JsonConvert.DeserializeObject<BadRequestResponse> value
            match response with
            | v when v.error.code = "PrincipalNotFound" -> Code PrincipalNotFound
            | _ -> Code Unknown

        let toRoleAssignment (ra:CreateRoleAssigment):RoleAssignment = {
            id = ra.id
            name = ra.name
            principalId = ra.properties.principalId
            principalName = ""
            roleDefinitionId = ra.properties.roleDefinitionId
            roleDefinitionName = ""
            scope = ra.properties.scope
            _type = ra._type
        }

        match get assigneeObjectId role scope with
        | Some roleAssignment -> RA roleAssignment
        | _ ->
            let s = scope.Replace(sprintf "/subscription/%s/" subscriptionId,"") 
            let url = 
                sprintf "%s/providers/Microsoft.Authorization/roleAssignments/%s?api-version=2018-01-01-preview" s (Guid.NewGuid().ToString())

            let accessTokenResult = getAccessToken "https://management.azure.com"

            let result = put Management url (Some accessTokenResult.accessToken) 
                            (Some (box {properties = {
                                roleDefinitionId = role
                                principalId = assigneeObjectId
                            }
                            }))
            match (result) with
                   |(Net.HttpStatusCode.Created, value)|(Net.HttpStatusCode.OK, value) ->
                        RA (toRoleAssignment <| JsonConvert.DeserializeObject<CreateRoleAssigment> value)
                   |(Net.HttpStatusCode.BadRequest, value) -> decodeBadRequest value
                   |(statusCode, value) -> failwithf "Could not create roleAssignments. Status code is %A. Content: %s" statusCode value
    
    let createWithRetry objId role (scope:string) =

        let rec attemptCall n =
            if n < 36 then 
                debugfn "Trying to find Service principal with objectId = %s. %d attempt" objId n
                let result = create objId role scope
                match result with
                | RA roleAssignment -> roleAssignment
                | Code PrincipalNotFound -> 
                    Threading.Thread.Sleep 5000
                    attemptCall (n + 1)
                | Code Unknown ->  
                    failwithf "Create Service principal with objectId = %s returned unknown error code after %d attempts" objId n
            else
                failwithf "Service principal with objectId = %s not found after %d attempts" objId n
        attemptCall 1
    
