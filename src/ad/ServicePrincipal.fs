namespace Bluefin.Ad

open System
open Bluefin.Core
open Bluefin.Http
open Newtonsoft.Json
open System.Web

module ServicePrincipal =
    type KeyCredential = {
        additionalProperties: string
        customKeyIdentifier: string
        endDate: string
        keyId: string
        startDate: string
        [<JsonProperty("type")>]
        _type: string
        usage: string
        value: string
    }

    type ServicePrincipal = {
        accountEnabled: bool
        appDisplayName: string
        appId: string
        appOwnerTenantId: string
        appRoleAssignmentRequired: bool
        applicationTemplateId: string
        deletionTimestamp: string
        displayName: string
        errorUrl: string
        homePage: string
        keyCredentials: KeyCredential[]
        logoutUrl: string
        notificationEmailAddresses: string[]        
        objectId: string
        objectType: string
        [<JsonProperty("odata.metadata")>]
        odata_metadata: string
        [<JsonProperty("odata.type")>]
        odata_type: string
        servicePrincipalNames: string[]
        servicePrincipalType: string        
    }
    
    let get id =
        let sp = azArr ["ad";"sp";"show";"--id";id]
        JsonConvert.DeserializeObject<ServicePrincipal> sp

    type ExistQuery = {
        objectIds: string[]
        includeDirectoryObjectReferences: bool 
    }

    type ExistResponse = {
        value: Object[]
    }
    let exist objId =
        let deserialize value = 
            JsonConvert.DeserializeObject<ExistResponse> value

        let objectFound obj =
            obj.value.Length > 0

        let accessTokenResult = getAccessToken "https://graph.windows.net/"

        let result = post Graph "getObjectsByObjectIds?api-version=1.6" (Some accessTokenResult.accessToken) (Some (box {objectIds = [|objId|];includeDirectoryObjectReferences=true}))
        match (result) with
               |(Net.HttpStatusCode.OK, value) -> 
                    objectFound (deserialize value)              
               |(statusCode, value) -> failwithf "Failed to get sp. Status code is %A. Content: %s" statusCode value 

    let getByFilter objectType (filter:string) =
        let accessTokenResult = getAccessToken "https://graph.windows.net/"

        let url = sprintf "%s?$filter=%s&api-version=1.6" objectType <| HttpUtility.UrlEncode filter
        let result = Bluefin.Http.get Graph url (Some accessTokenResult.accessToken)
        match (result) with
               |(Net.HttpStatusCode.OK, response) -> response                                 
               |(statusCode, response) -> failwithf "Failed to get %s. Status code is %A. Content: %A" objectType statusCode response 

    let existByFilter objectType (filter:string) =
        let objectFound obj =
            obj.value.Length > 0
        getByFilter objectType filter |> objectFound
        
    let spNameExist name =
        let filter = sprintf "servicePrincipalNames/any(x:x eq '%s')" name 
        existByFilter "servicePrincipals" filter

    let applicationNameExist name =        
        let filter = sprintf "startswith(displayName,'%s')" name 
        existByFilter "applications" filter

    type Application = {
        objectId: string
        appId: string
        displayName: string
    }

    type GetApplicationResponse = {
        value: Application[]
    }

    let getApplicationByName name = 
        let filter = sprintf "startswith(displayName,'%s')" name 
        let response = getByFilter "applications" filter 
        response.value |> Array.head

    let getServicePrincipalByName name =
        let filter = sprintf "servicePrincipalNames/any(x:x eq '%s')" name  
        let response = getByFilter "servicePrincipals" filter 
        response.value |> Array.head

    type CreateForRbacResponse = {
        appId: string
        displayName: string
        name: string
        password: string
        tenant: string
    }

    type PasswordCredentials = {
        startDate: string
        endDate: string
        keyId: string
        value: string
        customKeyIdentifier: string
    }    
    type CreateApplicationValues = {
        availableToOtherTenants: bool
        homepage: string
        passwordCredentials: seq<PasswordCredentials>
        displayName: string
        identifierUris: seq<string>
    }

    let private createApplication name spName password : Application =
        let deserialize value = 
            JsonConvert.DeserializeObject<Application> value

        let accessTokenResult = getAccessToken "https://graph.windows.net/"

        let now = DateTime.Now
        let result = post Graph "applications?api-version=1.6" 
                        (Some accessTokenResult.accessToken) 
                        (Some (box {
                            availableToOtherTenants = false
                            homepage = spName
                            passwordCredentials = [|{
                                startDate = now.ToString("o")
                                endDate = now.AddYears(1).ToString("o")
                                keyId = Guid.NewGuid().ToString()
                                value = password
                                customKeyIdentifier = "//5yAGIAYQBjAA=="
                            }|]
                            displayName = name
                            identifierUris = [|spName|]
                        }))
        match (result) with
               |(Net.HttpStatusCode.Created, value) | (Net.HttpStatusCode.OK, value) -> 
                    deserialize value              
               |(statusCode, value) -> failwithf "Failed to create application. Status code is %A. Content: %s" statusCode value

    type CreateServicePrincipal = {
        accountEnabled: bool
        appId: string
    }
    let createServicePrincipal appId = 
        let accessTokenResult = getAccessToken "https://graph.windows.net/"

        let result = post Graph "servicePrincipals?api-version=1.6" 
                        (Some accessTokenResult.accessToken) 
                        (Some (box {
                            accountEnabled = true
                            appId = appId
                        }))
        match (result) with
               |(Net.HttpStatusCode.Created, value) | (Net.HttpStatusCode.OK, value) -> 
                    JsonConvert.DeserializeObject<Application> value 
                               
               |(statusCode, value) -> failwithf "Failed to create service principal. Status code is %A. Content: %s" statusCode value

    type ResetCredentialsResponse = {
        appId: string
        name: string
        password: string
        tenant: string
    }
    let resetCredentials appId = 
        let r = azArr ["ad";"sp";"credential";"reset";
                         "-n";appId;] |>
                JsonConvert.DeserializeObject<ResetCredentialsResponse>
        r.password 

    type CreateForRbacResult = {
        appId: string
        displayName: string
        name: string
        password: string
        appObjectId: string
        spObjectId: string
    }

    let rec ensureServicePrincipalAvailable objId n =
        printfn "Ensuring Service principal with objectId = %s. available" objId 
        if n < 36 then 
            if (exist objId) then () else 
                Threading.Thread.Sleep 5000
                printfn "Trying to find Service principal with objectId = %s. %d attempt" objId (n + 1)
                ensureServicePrincipalAvailable objId (n + 1)
        else
            failwithf "Service principal with objectId = %s not found after %d attempts" objId n
        
    let createForRbac (name:string)  = //role scope
        let spName = "http://" + name
        let buildResult (app:Application) name password spObjectId = {
            appId = app.appId
            displayName = app.displayName
            name = name
            password = password
            appObjectId = app.objectId
            spObjectId = spObjectId
        }

        if (not <| applicationNameExist name) then
            if (spNameExist spName) then
                failwithf "Service principal exists but application does not"
            printfn "creating application"
            let app = createApplication name spName <| Guid.NewGuid().ToString()              
            let sp = createServicePrincipal app.appId
            let password = resetCredentials app.appId //ensure we use the same algorithm as azure cli
            buildResult app spName password sp.objectId
        else
            printfn "getting existing application"
            if (not <| spNameExist spName) then
                failwithf "Application exists but Service principal does not"
            let app = getApplicationByName name
            let sp = getServicePrincipalByName spName
            let password = resetCredentials app.appId
            buildResult app spName password sp.objectId

    let createForRbacWithRoles (name:string) (rs:seq<string*string>) = //role scope
        let app = createForRbac name
        rs 
        |> Seq.map (fun (role,scope) -> Bluefin.Role.Assignment.createWithRetry app.spObjectId role scope)         
        |> Seq.iter (printfn "roleresult: %A")
        app