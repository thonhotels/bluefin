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

    let addIpSecurityRestriction s = 
        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.Web/sites/%s/config/web?api-version=2018-02-01" s.resourceGroup s.appName

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let configRequest = {
            properties = {
                ipSecurityRestrictions = s.ipSecurityRestrictions
            }
        }

        let result = put url (Some accessTokenResult.accessToken) (Some <| box configRequest)

        match (result) with
               |(HttpStatusCode.OK, _) -> ()
               |(statusCode, value) -> failwithf "Could not add ip address restriction. Status code is %A. Content: %s" statusCode value