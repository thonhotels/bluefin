namespace Bluefin.Network

module Common =
    let resourceId rg resourceType name = 
        (Bluefin.Group.rgId rg) + "/providers/Microsoft.Network/" + (sprintf "%s/%s" resourceType name)
    
    