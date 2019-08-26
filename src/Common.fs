namespace Bluefin

open Fake.Core

module Common =

    let private tagToString (key, value) = 
        sprintf "%s=%s" key value

    let tagsToString tags =    
        if Seq.isEmpty tags then ""
        else (tags |> Seq.map tagToString |> String.concat " ")
    
    let tagsToArgs tags = 
       if Seq.isEmpty tags then [] else ["--tags";tagsToString tags]