namespace Bluefin

open Bluefin.Core

module Servicebus =
    let getQueueConnectionString rg namespaceName queueName = 
        az (sprintf "servicebus queue authorization-rule keys list -g %s --namespace-name %s --queue-name %s --name read-write --query primaryConnectionString" rg namespaceName queueName)

    let getTopicConnectionString rg namespaceName topicName = 
        az (sprintf "servicebus topic authorization-rule keys list -g %s --namespace-name %s --topic-name %s --name read-write --query primaryConnectionString" rg namespaceName topicName)        
