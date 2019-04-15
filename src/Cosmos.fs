namespace Bluefin

open Bluefin.Core

module Cosmos =
    type CosmosDbDatabaseOptions = {
        DatabaseName: string
        UrlConnection: string
        Key: string
    }

    type CosmosDbCollectionOptions = {
        CollectionName: string
        Throughput: int
        PartitionKey: string
        DefaultTtl: int
    }

    let getCosmosDbKey rg name = 
        az (sprintf "cosmosdb list-keys --name %s --resource-group %s --query primaryMasterKey" name rg)

    let cosmosDatabaseExists cosmosDbDatabaseOptions =
        sprintf "cosmosdb database exists -d %s --url-connection %s --key %s" cosmosDbDatabaseOptions.DatabaseName cosmosDbDatabaseOptions.UrlConnection cosmosDbDatabaseOptions.Key
        |> fun cmd ->     
             azRedact cmd (redactValue cmd cosmosDbDatabaseOptions.Key)
    
    let cosmosCollectionExists cosmosDbDatabaseOptions cosmosDbCollectionOptions = 
        sprintf "cosmosdb collection exists -d %s --url-connection %s --key %s -c %s" cosmosDbDatabaseOptions.DatabaseName cosmosDbDatabaseOptions.UrlConnection cosmosDbDatabaseOptions.Key cosmosDbCollectionOptions.CollectionName
        |>  fun cmd -> 
            azRedact cmd (redactValue cmd cosmosDbDatabaseOptions.Key) 
    
    let createCosmosDatabase cosmosDbDatabaseOptions = 
        let databaseExists = cosmosDatabaseExists cosmosDbDatabaseOptions
                
        if (databaseExists = "false") then
            sprintf "cosmosdb database create -d %s --url-connection %s --key %s" cosmosDbDatabaseOptions.DatabaseName cosmosDbDatabaseOptions.UrlConnection cosmosDbDatabaseOptions.Key
            |> fun cmd ->
                azRedact cmd (redactValue cmd cosmosDbDatabaseOptions.Key) 
                |> ignore

    let createCosmosCollection cosmosDbDatabaseOptions cosmosDbCollectionOptions = 
        let collectionExists = cosmosCollectionExists cosmosDbDatabaseOptions cosmosDbCollectionOptions
        
        if (collectionExists = "false") then
            sprintf "cosmosdb collection create -d %s --url-connection %s --key %s -c %s --throughput %i --partition-key-path %s --default-ttl %i" cosmosDbDatabaseOptions.DatabaseName cosmosDbDatabaseOptions.UrlConnection cosmosDbDatabaseOptions.Key cosmosDbCollectionOptions.CollectionName cosmosDbCollectionOptions.Throughput cosmosDbCollectionOptions.PartitionKey cosmosDbCollectionOptions.DefaultTtl
            |> fun cmd ->
                azRedact cmd (redactValue cmd cosmosDbDatabaseOptions.Key) 
                |> ignore