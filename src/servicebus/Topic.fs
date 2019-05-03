namespace Bluefin.Servicebus

open System
open Bluefin.Core

module Topic =
    let getConnectionString rg namespaceName topicName = 
        az (sprintf "servicebus topic authorization-rule keys list -g %s --namespace-name %s --topic-name %s --name read-write --query primaryConnectionString" rg namespaceName topicName)        

    type MaxSize = MB_1024 | MB_2048 | MB_3072 | MB_4096 | MB_5120
    let sizeToInt s =
        Int32.Parse (s.ToString().Replace("MB_", ""))

    type Status = Active | Disabled | ReceiveDisabled | SendDisabled

    type TopicSettings = {
        namespaceName: string
        autoDeleteOnIdle: TimeSpan                      // idle interval after which the topic is
                                                        // automatically deleted. The minimum duration is 5
                                                        // minutes
        defaultMessageTimeToLive: TimeSpan              // default message timespan to live value. This is the
                                                        // duration after which the message expires,
                                                        // starting from when the message is sent to
                                                        // Service Bus. This is the default value used when
                                                        // TimeToLive is not set on a message itself
        duplicateDetectionHistoryTimeWindow: TimeSpan   // the duration of the duplicate detection history. The
                                                        // default value is 10 minutes
        enableBatchedOperations: bool                   // Allow server-side batched operations
        enableDuplicateDetection: bool                  // Does topic require duplicate detection
        enableExpress: bool                             // Are Express Entities enabled. An express topic holds a
                                                        // message in memory temporarily before writing it
                                                        // to persistent storage
        enableOrdering: bool                            // indicates whether the topic supports ordering.
        enablePartitioning: bool                        // Is the topic to be partitioned across multiple message brokers
        maxSize: MaxSize                                // The maximum size of topic in megabytes, which is
                                                        // the size of memory allocated for the topic.
                                                        // Default is 1024
        status: Status                                  // Enumerates the possible values for the status of
                                                        // a messaging entity.  Allowed values: Active,
                                                        // Disabled, ReceiveDisabled, SendDisabled
    }   

    let days d = 
        TimeSpan (d, 0, 0, 0)
    let minutes m = 
        TimeSpan (0, m, 0)

    let defaultTopicSettings = {
        namespaceName = ""
        autoDeleteOnIdle = days 10675199
        defaultMessageTimeToLive = days 14
        duplicateDetectionHistoryTimeWindow = minutes 10
        enableBatchedOperations = true
        enableDuplicateDetection = false
        enableExpress = false
        enableOrdering = false
        enablePartitioning = false
        maxSize = MB_1024
        status = Active
    }

    let toDuration (t:TimeSpan) = 
        System.Xml.XmlConvert.ToString t 

    let create rg name q =
        let l1 = sprintf "servicebus topic create -g %s -n %s --namespace-name %s --auto-delete-on-idle %s --default-message-time-to-live %s "
                    rg name q.namespaceName (toDuration q.autoDeleteOnIdle) (toDuration q.defaultMessageTimeToLive)
        let l2 = sprintf "--duplicate-detection-history-time-window %s --enable-batched-operations %b --enable-duplicate-detection %b " 
                    (toDuration q.duplicateDetectionHistoryTimeWindow) q.enableBatchedOperations q.enableDuplicateDetection 
        let l3 = sprintf "--enable-express %b --enable-ordering %b --enable-partitioning %b --max-size %d --status %s" 
                    q.enableExpress q.enableOrdering q.enablePartitioning (sizeToInt q.maxSize) (q.status.ToString())

        az (l1 + l2 + l3) 
  