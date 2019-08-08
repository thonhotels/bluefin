namespace Bluefin.Servicebus

open Bluefin.Core
open Bluefin.Http
open System.Net
open Microsoft.FSharp.Core
open System

module Subscription =
    type SubscriptionArgs = {
        autoDeleteOnIdle: TimeSpan option 
                            // Idle interval after which the topic is automatically deleted. 
                            // The minimum duration is 5 minutes.
        deadLetteringOnFilterEvaluationExceptions: bool
                            // Indicates whether a subscription has dead letter support 
                            // on filter evaluation exceptions.
        deadLetteringOnMessageExpiration: bool 
                            // Indicates whether a subscription has dead letter support when a message expires                                                 
        defaultMessageTimeToLive: TimeSpan option  
                            // Default message timespan to live value. This is the duration 
                            // after which the message expires, starting from when the message is 
                            // sent to Service Bus. This is the default value used when TimeToLive is not 
                            // set on a message itself.
        duplicateDetectionHistoryTimeWindow: TimeSpan option
                            // defines the duration of the duplicate detection history. 
                            // The default value is 10 minutes.
        enableBatchedOperations: bool
                            // Indicates whether server-side batched operations are enabled.
        forwardDeadLetteredMessagesTo: string option
                            // Queue/Topic name to forward the Dead Letter message.
        forwardTo: string option
                            // Queue/Topic name to forward the messages
        lockDuration: TimeSpan
                            // lock duration for the subscription. The default value is 1 minute.        
        maxDeliveryCount: int
                            // Number of maximum deliveries.
        requiresSession: bool
                            // Value indicating if a subscription supports the concept of sessions.                                                   
    }

    let defaultSubscription = {
        autoDeleteOnIdle = None
        deadLetteringOnFilterEvaluationExceptions = false
        deadLetteringOnMessageExpiration = true                                              
        defaultMessageTimeToLive = Some (TimeSpan (14,0,0,0))
        duplicateDetectionHistoryTimeWindow = None
        enableBatchedOperations = false
        forwardDeadLetteredMessagesTo = None
        forwardTo = None
        lockDuration = TimeSpan (0, 1, 0)     
        maxDeliveryCount = 10
        requiresSession = false
    }

    type Body = {
        properties: SubscriptionArgs 
    }
    let create rg namespaceName topicName subscriptionName (args:SubscriptionArgs)=

        let url = 
            sprintf "resourceGroups/%s/providers/Microsoft.ServiceBus/namespaces/%s/topics/%s/subscriptions/%s?api-version=2017-04-01" rg namespaceName topicName subscriptionName

        let accessTokenResult = getAccessToken "https://management.azure.com"

        let result = put Management url (Some accessTokenResult.accessToken) (Some (box {properties = args}))
        match (result) with
               |(HttpStatusCode.OK, value) | (HttpStatusCode.Accepted, value) -> printfn "Updated servicebus subscription"
               |(HttpStatusCode.Created, value) -> printfn "Created servicebus subscription"
               |(statusCode, value) -> failwithf "Could not create servicebus subscription. Status code is %A. Content: %s" statusCode value

        ()
