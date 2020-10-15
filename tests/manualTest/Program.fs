open System
open Fake.Core
open Bluefin
open Bluefin.Cosmos

[<EntryPoint>]
let main args = 
    let cosmosDbAccountOptions: Bluefin.Cosmos.CosmosDbOptions =
        {
            ResourceGroupName = args.[0]
            DbAccountName = args.[1]
            DbName = args.[2]
        }

    let cosmosDbCollectionOptions: Bluefin.Cosmos.CosmosDbCollectionOptions =
        {
            CollectionName = args.[3]
            DefaultTtl = Int32.Parse (args.[6])
            PartitionKeyPath = args.[5]
            Throughput = Int32.Parse (args.[4])
        }

    // let create = Cosmos.createCosmosCollection cosmosDbAccountOptions cosmosDbCollectionOptions

    Trace.trace (Cosmos.getCosmosDbKey args.[0] args.[1])

    Trace.trace (sprintf "Create collection ResourceGroupName = '%s', DbAccountName = '%s', DbName = '%s', CollectionName = '%s' and Throughput '%s' and PartitionKeyPath = '%s' and Ttl = '%s'" args.[0] args.[1] args.[2] args.[3] args.[4] args.[5] args.[6])
    
    0