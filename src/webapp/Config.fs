namespace Bluefin.Webapp

open Bluefin.Http
open System.Net
open Bluefin.Core

module Config = 
    
    type IpSecurityRestrictionActionType = Allow = 0 | Deny = 1

    type IpSecurityRestriction = {
        ipAddress: string
        action: IpSecurityRestrictionActionType
        tag: string
        priority: int
        name: string
    }

    type IpSecurityRestrictionArgs = {
         resourceGroup: string
         appName: string
         ipSecurityRestrictions: seq<IpSecurityRestriction>
    }

    type Properties = {
        ipSecurityRestrictions: seq<IpSecurityRestriction>
    }
    
    type ConfigRequest = {
        properties: Properties
    }

    type Settings = {
        alwaysOn: bool          // Ensure web app gets loaded all the time, rather unloaded after been idle
        http20Enabled: bool     // Configures a web site to allow clients to connect over http2.0.
        use32BitWorkerProcess: bool // Use 32 bits worker process or not.
        webSocketsEnabled: bool // Enable or disable web sockets.        
    }

    let defaultSettings = {
        alwaysOn = true
        http20Enabled = true
        use32BitWorkerProcess = true
        webSocketsEnabled = false
    }

    let set rg name (a:Settings) =
        azArr ["webapp";"config";"set";"-g";rg;"-n";name;
                "--always-on";a.alwaysOn.ToString();
                "--remote-debugging-enabled";"false";
                "--http20-enabled"; a.http20Enabled.ToString();
                "--use-32bit-worker-process";a.use32BitWorkerProcess.ToString();
                "--web-sockets-enabled";a.webSocketsEnabled.ToString()
              ] |> ignore 

    let addIpSecurityRestriction s = 
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.Web/sites/%s/config/web?api-version=2018-02-01" s.resourceGroup s.appName

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let configRequest = {
            properties = {
                ipSecurityRestrictions = s.ipSecurityRestrictions
            }
        }

        let result = put Management url (Some accessTokenResult.accessToken) (Some <| box configRequest)

        match (result) with
               |(HttpStatusCode.OK, _) -> ()
               |(statusCode, value) -> failwithf "Could not add ip address restriction. Status code is %A. Content: %s" statusCode value