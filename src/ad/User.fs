namespace Bluefin.Ad

open Bluefin.Core
open System
open Newtonsoft.Json

module User =
    //ype UserType = Guest | Member

    type User = {
        accountEnabled: bool
        createdDateTime: DateTime
        creationType: string
        department: string
        displayName: string
        objectId: string
        objectType: string
        userPrincipalName: string
        userType: string
    }

    // finds user by display name. Will return all users with displayname starting with the given name
    let findByNameStartsWith name =
        let r = az (sprintf "ad user list --display-name %s" name)
        JsonConvert.DeserializeObject<seq<User>> r