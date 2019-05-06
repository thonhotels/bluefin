namespace Bluefin.Sql

open Bluefin.Core

module Db =
    //ype UserType = Guest | Member

    type ElasticPoolDb = {
        server: string      //Name of the Azure SQL server
        elasticPool: string // The name or resource id of the elastic pool to create the database in
        collation: string   // The collation of the database
        maxSize: string        // The max storage size in bytes
    }

    let createElasticPoolDb rg name db =
        az (sprintf "sql db create -g %s -n %s -s %s --elastic-pool %s --collation %s --max-size %s" rg name db.server db.elasticPool db.collation db.maxSize) |> ignore