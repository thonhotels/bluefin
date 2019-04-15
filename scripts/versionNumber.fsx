module VersionNumber

open System.Text.RegularExpressions

#if !FAKE
  #load ".fake/build.fsx/intellisense.fsx"
#endif    

let getFromGit () : string option = 
    let tag = Fake.Tools.Git.Information.describe "."
    let (|Regex|_|) pattern tag =
        let m = Regex.Match(tag, pattern)
        if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None
    match tag with
    | Regex @"^v\d*\.\d*\.(?<buildnumber>\d*)$" [ buildnumber; ] -> Some buildnumber
    | _ -> None