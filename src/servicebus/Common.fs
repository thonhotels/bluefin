namespace Bluefin.Servicebus

open System

module Common =
    type MaxSize = MB_1024 | MB_2048 | MB_3072 | MB_4096 | MB_5120
    let sizeToInt s =
        Int32.Parse (s.ToString().Replace("MB_", ""))

    type Status = Active | Disabled | ReceiveDisabled | SendDisabled

    let toDuration (t:TimeSpan) = 
        Xml.XmlConvert.ToString t 

    type Right = Listen | Manage | Send

    type AuthorizationRule = {
        namespaceName: string   // Name of Namespace
        name: string            // Name of Authorization Rule
        rights: seq<Right>      // list of Authorization rule rights
    }    

    let rightsToString r = 
        r |>
        Seq.map (fun right -> right.ToString()) |>
        String.concat " "

    let resourceId rg resourceType name = 
        (Bluefin.Group.rgId rg) + "/providers/Microsoft.ServiceBus/" + (sprintf "%s/%s" resourceType name)