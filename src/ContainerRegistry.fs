namespace Bluefin

open Core

module ContainerRegistry =
    let resourceId rg name =
        (Group.rgId rg) + (sprintf "/providers/Microsoft.ContainerRegistry/registries/%s" name)
    
    let login name =
        azArr [
            "acr"
            "login"
            "--name"
            name
        ] |> ignore