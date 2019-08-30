namespace Bluefin

module ContainerRegistry =
    let resourceId rg name =
        (Group.rgId rg) + (sprintf "/providers/Microsoft.ContainerRegistry/registries/%s" name)