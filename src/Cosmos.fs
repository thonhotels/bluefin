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
        azArr ["cosmosdb";"list-keys";"--name";name;"--resource-group";rg;"--query";"primaryMasterKey"] 

    let cosmosDatabaseExists cosmosDbDatabaseOptions =    
        let databaseExists = 
            azRedact [
                "cosmosdb"
                "database"
                "exists"
                "-d"
                cosmosDbDatabaseOptions.DatabaseName
                "--url-connection"
                cosmosDbDatabaseOptions.UrlConnection
                "--key"
                cosmosDbDatabaseOptions.Key]
                (sprintf "cosmosdb database exists -d %s --url-connection %s --key ***" 
                    cosmosDbDatabaseOptions.DatabaseName
                    cosmosDbDatabaseOptions.UrlConnection)
        databaseExists = "true"
  
    let cosmosCollectionExists cosmosDbDatabaseOptions cosmosDbCollectionOptions = 
        let collectionExists = 
            azRedact [
                "cosmosdb"
                "collection"
                "exists"
                "-d"
                cosmosDbDatabaseOptions.DatabaseName
                "--url-connection"
                cosmosDbDatabaseOptions.UrlConnection
                "--key"
                cosmosDbDatabaseOptions.Key
                "-c"
                cosmosDbCollectionOptions.CollectionName] 
                (sprintf "cosmosdb collection exists -d %s --url-connection %s --key *** -c %s" 
                    cosmosDbDatabaseOptions.DatabaseName 
                    cosmosDbDatabaseOptions.UrlConnection 
                    cosmosDbCollectionOptions.CollectionName)
        collectionExists = "true"

    
    let createCosmosDatabase cosmosDbDatabaseOptions = 
        let databaseExists = cosmosDatabaseExists cosmosDbDatabaseOptions
                
        if not databaseExists then
            azRedact 
                ["cosmosdb"
                 "database"
                 "create"
                 "-d"
                 cosmosDbDatabaseOptions.DatabaseName
                 "--url-connection"
                 cosmosDbDatabaseOptions.UrlConnection
                 "--key"
                 cosmosDbDatabaseOptions.Key] 
                (sprintf "cosmosdb database create -d %s --url-connection %s --key ***" 
                    cosmosDbDatabaseOptions.DatabaseName 
                    cosmosDbDatabaseOptions.UrlConnection)
            |> ignore

    let createCosmosCollection cosmosDbDatabaseOptions cosmosDbCollectionOptions = 
        let collectionExists = cosmosCollectionExists cosmosDbDatabaseOptions cosmosDbCollectionOptions
        
        if not collectionExists then
            azRedact 
                ["cosmosdb"
                 "collection"
                 "create"
                 "-d"
                 cosmosDbDatabaseOptions.DatabaseName
                 "--url-connection"
                 cosmosDbDatabaseOptions.UrlConnection
                 "--key"
                 cosmosDbDatabaseOptions.Key
                 "-c"
                 cosmosDbCollectionOptions.CollectionName
                 "--throughput"
                 string cosmosDbCollectionOptions.Throughput
                 "--partition-key-path"
                 cosmosDbCollectionOptions.PartitionKey
                 "--default-ttl"
                 string cosmosDbCollectionOptions.DefaultTtl
                 ] 
                (sprintf "cosmosdb collection create -d %s --url-connection %s --key **** -c %s --throughput %i --partition-key-path %s --default-ttl %i" 
                    cosmosDbDatabaseOptions.DatabaseName 
                    cosmosDbDatabaseOptions.UrlConnection 
                    cosmosDbCollectionOptions.CollectionName 
                    cosmosDbCollectionOptions.Throughput 
                    cosmosDbCollectionOptions.PartitionKey 
                    cosmosDbCollectionOptions.DefaultTtl)
            |> ignore