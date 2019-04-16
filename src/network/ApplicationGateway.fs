namespace Bluefin.Network

open Bluefin.Core

module ApplicationGateway =
    type Sku = 
        | Standard_Large
        | Standard_Medium 
        | Standard_Small 
        | Standard_v2
        | WAF_Large
        | WAF_Medium
        | WAF_v2
        
        //.  Default: Standard_Medium
    let createApplicationGateway rg name (sku:Sku) capacity subnet vnetName publicIpAddress httpSettingsPort httpSettingsProtocol = 
        let l1 = sprintf "network application-gateway create -g %s -n %s --sku %s " rg name (sku.ToString())
        let l2 = sprintf "--capacity %d --subnet %s --vnet-name %s --public-ip-address %s " capacity subnet vnetName publicIpAddress
        let l3 = sprintf "--http-settings-port %s --http-settings-protocol %s" httpSettingsPort httpSettingsProtocol
        az (l1 + l2 + l3)

    let createFrontendPort rg name gatewayName port =
        az (sprintf "network application-gateway frontend-port create -g %s -n %s --gateway-name %s --port %s" rg name gatewayName port)
    
    let createAddressPool rg name gatewayName servers =
        az (sprintf "network application-gateway address-pool create -g %s -n %s --gateway-name %s --servers %s" rg name gatewayName servers)

    let createHttpListener rg name gatewayName frontendPort frontendIp =
        az (sprintf "network application-gateway http-listener create -g %s -n %s --gateway-name %s --frontend-port %s --frontend-ip %s" rg name gatewayName frontendPort frontendIp)
    
    let createRules rg name gatewayName listener settings addressPool = 
        let listenerArg = 
            match listener with
            | Some l -> "--http-listener " + l
            | None  -> ""
        
        let settingsArg =
            match settings with
            | Some s -> "--http-settings " + s
            | None  -> ""
        
        let addressPoolArg = 
            match addressPool with
            | Some a -> "--address-pool " + a
            | None  -> ""

        az ((sprintf "network application-gateway rule create -g %s -n %s --gateway-name %s " rg name gatewayName) +
                    listenerArg + settingsArg + addressPoolArg)