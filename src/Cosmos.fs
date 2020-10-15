namespace Bluefin

open Bluefin.Core

module Cosmos =
    type CosmosDbOptions = {
        DbAccountName: string
        DbName: string
        ResourceGroupName: string
    }

    type CosmosDbCollectionOptions = {
        CollectionName: string
        DefaultTtl: int
        PartitionKeyPath: string
        Throughput: int
    }

    let getCosmosDbKey rg name =
        azArr
            [
                "cosmosdb"
                "keys"
                "list"
                "-g"
                rg
                "-n"
                name
                "--query"
                "primaryMasterKey"
            ]

    let createCosmosCollection cosmosDbOptions cosmosDbCollectionOptions =
        azRedact
            [
                "cosmosdb"
                "create"
                "-g"
                cosmosDbOptions.ResourceGroupName
                "-n"
                cosmosDbOptions.DbAccountName
            ]
            (
                sprintf "az cosmosdb create -g %s -n %s"
                    cosmosDbOptions.ResourceGroupName
                    cosmosDbOptions.DbAccountName
            )
        |> ignore

        azRedact
            [
                "cosmosdb"
                "sql"
                "database"
                "create"
                "-g"
                cosmosDbOptions.ResourceGroupName
                "--account-name"
                cosmosDbOptions.DbAccountName
                "-n"
                cosmosDbOptions.DbName
            ]
            (
                sprintf "az cosmosdb sql database create -g %s --account-name %s -n %s"
                    cosmosDbOptions.ResourceGroupName
                    cosmosDbOptions.DbAccountName
                    cosmosDbOptions.DbName
            )
        |> ignore

        azRedact
            [
                "cosmosdb"
                "sql"
                "container"
                "create"
                "-g"
                cosmosDbOptions.ResourceGroupName
                "-a"
                cosmosDbOptions.DbAccountName
                "-d"
                cosmosDbOptions.DbName
                "-n"
                cosmosDbCollectionOptions.CollectionName
                "--throughput"
                string cosmosDbCollectionOptions.Throughput
                "--partition-key-path"
                cosmosDbCollectionOptions.PartitionKeyPath
                "--ttl"
                string cosmosDbCollectionOptions.DefaultTtl
             ] 
            (
                sprintf "az cosmosdb sql container create -g %s -a %s -d %s -n %s --throughput %i --partition-key-path %s --ttl %i" 
                    cosmosDbOptions.ResourceGroupName
                    cosmosDbOptions.DbAccountName
                    cosmosDbOptions.DbName
                    cosmosDbCollectionOptions.CollectionName
                    cosmosDbCollectionOptions.Throughput
                    cosmosDbCollectionOptions.PartitionKeyPath
                    cosmosDbCollectionOptions.DefaultTtl
            )
        |> ignore