namespace Bluefin

open System
open Bluefin.Core
open Bluefin.Common

module AlertRule =
    type AlertRuleSettings = {
        enabled: bool
        severity: string
        subscription: string option
        description: string
        tags: seq<string*string>
        conditions: seq<string>
        scopes: seq<string>
        windowSize: string
        evaluationFrequency: string
        actionGroup: string
    }
    
    let defaultAlertRuleSettings = {
        enabled = true
        severity = "2"
        subscription = None
        description = String.Empty
        tags = [||]
        conditions = [||]
        scopes = [||]
        windowSize = "5m"
        evaluationFrequency = "1m"
        actionGroup = String.Empty
    }
    
    let createAlertRule rg name settings =
        let conditions =
            if Seq.isEmpty settings.conditions then []
            else Seq.collect(fun condition -> ["--condition"; condition]) settings.conditions |> Seq.toList
        
        let scopes =
            if Seq.isEmpty settings.scopes then []
            else ["--scopes"; settings.scopes |> String.concat " "]
        
        let arr = ["monitor";"metrics";"alert";"create"
                   "-g";rg
                   "-n";name
                   "--severity"; settings.severity
                   "--evaluation-frequency"; settings.evaluationFrequency
                   "--window-size"; settings.windowSize
                   "--description"; settings.description
                   ]
                    @ match settings.subscription with | Some subscription -> ["--subscription";subscription] | None -> []
                    @ if String.IsNullOrEmpty settings.actionGroup then [] else ["-a"; settings.actionGroup]
                    @ tagsToArgs settings.tags
                    @ conditions
                    @ scopes
        
        azArr arr