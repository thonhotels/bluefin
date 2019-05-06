namespace Bluefin.Servicebus

open System
open Bluefin.Core
open Bluefin.Servicebus.Common

module Queue =
    let getConnectionString rg namespaceName queueName = 
        az (sprintf "servicebus queue authorization-rule keys list -g %s --namespace-name %s --queue-name %s --name read-write --query primaryConnectionString" rg namespaceName queueName)

    type QueueSettings = {
        namespaceName: string
        autoDeleteOnIdle: TimeSpan                      // idle interval after which the queue is
                                                        // automatically deleted. The minimum duration is 5
                                                        // minutes
        defaultMessageTimeToLive: TimeSpan              // default message to live value. This is the
                                                        // duration after which the message expires,
                                                        // starting from when the message is sent to
                                                        // Service Bus. This is the default value used when
                                                        // TimeToLive is not set on a message itself
        duplicateDetectionHistoryTimeWindow: TimeSpan   // the duration of the duplicate detection history. The
                                                        // default value is 10 minutes
        enableBatchedOperations: bool                   // Allow server-side batched operations
        enableDeadLetteringOnMessageExpiration: bool    // Has queue dead letter support when a message expires
        enableDuplicateDetection: bool                  // Does queue require duplicate detection
        enableExpress: bool                             // Are Express Entities enabled. An express queue holds a
                                                        // message in memory temporarily before writing it
                                                        // to persistent storage
        enablePartitioning: bool                        // Is the queue to be partitioned across multiple message brokers
        enableSession: bool                             // Does the queue supports the concept of sessions.
        forwardDeadLetteredMessagesTo: string           // Queue/Topic name to forward the Dead Letter message
        forwardTo: string                               // Queue/Topic name to forward the messages
        lockDuration: TimeSpan                          // duration of a peek-lock; that is, the amount of
                                                        // time that the message is locked for other
                                                        // receivers. The maximum value for LockDuration is
                                                        // 5 minutes; the default value is 1 minute.
        maxDeliveryCount: int                           // The maximum delivery count. A message is
                                                        // automatically deadlettered after this number of
                                                        // deliveries. default value is 10
        maxSize: MaxSize                                // The maximum size of queue in megabytes, which is
                                                        // the size of memory allocated for the queue.
                                                        // Default is 1024
        status: Status                                  // Enumerates the possible values for the status of
                                                        // a messaging entity.  Allowed values: Active,
                                                        // Disabled, ReceiveDisabled, SendDisabled
    }   

    let days d = 
        TimeSpan (d, 0, 0, 0)
    let minutes m = 
        TimeSpan (0, m, 0)

    let defaultQueueSettings = {
        namespaceName = ""
        autoDeleteOnIdle = days 10675199
        defaultMessageTimeToLive = days 14
        duplicateDetectionHistoryTimeWindow = minutes 10
        enableBatchedOperations = true
        enableDeadLetteringOnMessageExpiration = false
        enableDuplicateDetection = false
        enableExpress = false
        enablePartitioning = false
        enableSession = false
        forwardDeadLetteredMessagesTo = ""
        forwardTo = ""
        lockDuration = minutes 1
        maxDeliveryCount = 10
        maxSize = MB_1024
        status = Active
    }

    let createAuthorizationRule rg name (r:AuthorizationRule) =
        az (sprintf "servicebus queue authorization-rule create -g %s --namespace-name %s --queue-name %s -n %s --rights %s" rg r.namespaceName name r.name (rightsToString r.rights))

    let create rg name q =
        let forwardDlqArg = 
            if String.IsNullOrEmpty q.forwardDeadLetteredMessagesTo then ""
            else sprintf "--forward-dead-lettered-messages-to %s" q.forwardDeadLetteredMessagesTo

        let forwardToArg = 
            if String.IsNullOrEmpty q.forwardTo then ""
            else sprintf "--forward-to %s" q.forwardTo

        let l1 = sprintf "servicebus queue create -g %s -n %s --namespace-name %s --auto-delete-on-idle %s --default-message-time-to-live %s "
                    rg name q.namespaceName (toDuration q.autoDeleteOnIdle) (toDuration q.defaultMessageTimeToLive)
        let l2 = sprintf "--duplicate-detection-history-time-window %s --enable-batched-operations %b --enable-dead-lettering-on-message-expiration %b " 
                    (toDuration q.duplicateDetectionHistoryTimeWindow) q.enableBatchedOperations q.enableDeadLetteringOnMessageExpiration
        let l3 = sprintf "--enable-duplicate-detection %b --enable-express %b --enable-partitioning %b --enable-session %b " 
                    q.enableDuplicateDetection q.enableExpress q.enablePartitioning q.enableSession
        let l4 = sprintf "%s %s --lock-duration %s "
                    forwardDlqArg forwardToArg (toDuration q.lockDuration)
        let l5 = sprintf "--max-delivery-count %d  --max-size %d --status %s" 
                    q.maxDeliveryCount (sizeToInt q.maxSize) (q.status.ToString())
                 
        az (l1 + l2 + l3 + l4 + l5) 

         