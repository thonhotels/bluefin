namespace Bluefin.Servicebus

open Bluefin.Core
open Bluefin.Http
open System.Net
open Microsoft.FSharp.Core
open System

module Rule =

    type Action = {
       compatibilityLevel: int
       requiresPreprocessing: bool
       sqlExpression: string
    }

    type CorrelationFilter = {
        contentType: string option
        correlationId: string option
        label: string option
        messageId: string option
        properties: Object option
        replyTo: string option
        replyToSessionId: string option
        requiresPreprocessing: bool option
        sessionId: string option
        ``to``: string option
    }

    type SqlFilter = {
        compatibilityLevel: int option
        requiresPreprocessing: bool option
        sqlExpression: string
    }

    type FilterType = CorrelationFilter | SqlFilter

    type RuleArgs = {
        action: Action option
                            // Represents the filter actions which are allowed for the 
                            // transformation of a message that have been matched by a filter expression.
        correlationFilter: CorrelationFilter option
        filterType: FilterType
        sqlFilter: SqlFilter option  
    }

    let defaultSqlRule = {
        action = None
        correlationFilter = None
        filterType = SqlFilter                                              
        sqlFilter = Some {
            compatibilityLevel =  None
            requiresPreprocessing = None
            sqlExpression = "1=1"
        }
    }

    let defaultCorrelationRule = {
        action = None
        correlationFilter = Some {
            contentType = None
            correlationId = None
            label = None
            messageId = None
            properties = None
            replyTo = None
            replyToSessionId = None
            requiresPreprocessing = None
            sessionId = None
            ``to`` = None
        }
        filterType = CorrelationFilter                                              
        sqlFilter = None
    }

    let buildSqlRule expression = 
         { 
            defaultSqlRule with 
                sqlFilter = Some {
                    compatibilityLevel = None
                    requiresPreprocessing = None
                    sqlExpression = expression
                }
         }

    type Body = {
        properties: RuleArgs 
    }
    let create rg namespaceName topicName subscriptionName ruleName (args:RuleArgs)=

        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.ServiceBus/namespaces/%s/topics/%s/subscriptions/%s/rules/%s?api-version=2017-04-01" rg namespaceName topicName subscriptionName ruleName

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put url (Some accessTokenResult.accessToken) (Some (box {properties = args}))
        match (result) with
               |(HttpStatusCode.OK, value) | (HttpStatusCode.Accepted, value) -> printfn "Updated servicebus subscription rule"
               |(HttpStatusCode.Created, value) -> printfn "Created servicebus subscription rule"
               |(statusCode, value) -> failwithf "Could not create servicebus subscription rule. Status code is %A. Content: %s" statusCode value

        ()
