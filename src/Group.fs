namespace Bluefin

open Bluefin.Core

module Group =
    let createResourceGroup name location =
        az (sprintf "group create --name %s --location %s" name location)
    
    let applyArmTemplate rg templatePath templateFile parameterFile = 
        azWorkingDir (sprintf "group deployment create -g %s --template-file %s --parameters %s" rg templateFile parameterFile) templatePath
            |> ignore