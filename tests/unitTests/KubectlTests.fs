module KubectlTests

open Bluefin.Kubernetes
open Xunit
open Fake.IO


[<Fact>]
let ``My test`` () =
    Kubectl.mergeYml "testfiles" "dev" ignore "test.yml" |> ignore
    GlobbingPattern.create "testfiles/*deploy.yml"
    |> File.deleteAll

[<Fact>]
let ``params files are not required`` () = 
  //assumes that test2.yml does not have a test2.dev.json file
  let getFullNames files : seq<string> = 
    Seq.map (fun f -> Path.getFullName "testfiles/" + f) files
    |> Seq.sort
    |> Seq.rev

  let testFileContent f = 
    File.readAsString ("testfiles/" + f)

  let filesAreEqual f1 f2 =
    testFileContent f1 = testFileContent f2

  GlobbingPattern.create "testfiles/*deploy.yml"
  |> File.deleteAll

  let mergeYmlFolder = Kubectl.mergeYml "testfiles" "dev" ignore
  
  [|"test.yml";"test2.yml"|]  
  |> Seq.iter mergeYmlFolder

  Assert.Equal<seq<string>>(getFullNames [|"test-deploy.yml";"test2-deploy.yml"|], GlobbingPattern.create "testfiles/*deploy.yml")
  Assert.False (filesAreEqual "test.yml" "test-deploy.yml") // parameter replacement
  Assert.True (filesAreEqual "test2.yml" "test2-deploy.yml") // no parameter file, so should be unchanged

  GlobbingPattern.create "testfiles/*deploy.yml"
  |> File.deleteAll