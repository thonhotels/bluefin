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
    
    type FrontendPort = 
        { name: string
          number: int }

    type AddressPool = 
        { name: string
          servers: seq<string> }

    type HttpListener = 
        { name: string
          frontendPortNameOrId: string
          frontendIpNameOrId: string }

    type RoutingRule =
        { name: string
          listener: string option
          settings: string option
          addressPool: string option }
    
    type HttpProtocol = Http | Https

    type ApplicationGateway = 
        { resourceGroup: string
          name: string
          sku: Sku
          capacity: int
          subnet: string
          vnetName: string
          publicIpAddress: string
          httpSettingsPort: int
          httpSettingsProtocol: HttpProtocol
          frontendPorts: seq<FrontendPort>
          backendAddressPools: seq<AddressPool>
          httpListeners: seq<HttpListener>
          rules: seq<RoutingRule>
    }

    let createApplicationGatewayBasic rg name (sku:Sku) capacity subnet vnetName publicIpAddress httpSettingsPort (httpSettingsProtocol:HttpProtocol) = 
        let l1 = sprintf "network application-gateway create -g %s -n %s --sku %s " rg name (sku.ToString())
        let l2 = sprintf "--capacity %d --subnet %s --vnet-name %s --public-ip-address %s " capacity subnet vnetName publicIpAddress
        let l3 = sprintf "--http-settings-port %d --http-settings-protocol %s" httpSettingsPort (httpSettingsProtocol.ToString())
        az (l1 + l2 + l3) 

    let createFrontendPort rg gatewayName (p:FrontendPort) =
        az (sprintf "network application-gateway frontend-port create -g %s -n %s --gateway-name %s --port %d" rg p.name gatewayName p.number)
    
        
    let createAddressPool rg gatewayName (p:AddressPool) =
        az (sprintf "network application-gateway address-pool create -g %s -n %s --gateway-name %s --servers %s" rg p.name gatewayName (String.concat " " p.servers))
        

    let createHttpListener rg gatewayName (h:HttpListener) =
        az (sprintf "network application-gateway http-listener create -g %s -n %s --gateway-name %s --frontend-port %s --frontend-ip %s" rg h.name gatewayName h.frontendPortNameOrId h.frontendIpNameOrId)
    
    let createRules rg gatewayName r = 
        let listenerArg = 
            match r.listener with
            | Some l -> "--http-listener " + l
            | None  -> ""
        
        let settingsArg =
            match r.settings with
            | Some s -> "--http-settings " + s
            | None  -> ""
        
        let addressPoolArg = 
            match r.addressPool with
            | Some a -> "--address-pool " + a
            | None  -> ""

        az ((sprintf "network application-gateway rule create -g %s -n %s --gateway-name %s " rg r.name gatewayName) +
                    listenerArg + settingsArg + addressPoolArg)

    let createApplicationGateway (gw:ApplicationGateway) =
        createApplicationGatewayBasic gw.resourceGroup gw.name gw.sku gw.capacity gw.subnet gw.vnetName gw.publicIpAddress gw.httpSettingsPort gw.httpSettingsProtocol |> ignore
        
        Seq.map (createFrontendPort gw.resourceGroup gw.name) gw.frontendPorts |> ignore
        Seq.map (createAddressPool gw.resourceGroup gw.name) gw.backendAddressPools |> ignore
        Seq.map (createHttpListener gw.resourceGroup gw.name) gw.httpListeners |> ignore
        Seq.map (createRules gw.resourceGroup gw.name) gw.rules |> ignore
        

