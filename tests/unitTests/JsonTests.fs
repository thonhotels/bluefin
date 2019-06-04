module Tests

open Xunit
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open System

type Kind = BlobStorage | BlockBlobStorage | FileStorage | Storage | StorageV2

type StorageAccount = {
        kind: Kind
        test: string
    }

type ExpectedResult = {
    kind: string
    test: string
}
[<Fact>]
let ``My test`` () =
    //
    let myObj:StorageAccount = {
        kind = BlobStorage
        test = "hest"
    }
    let expected = {
        kind = "BlobStorage"
        test = "hest"
    }

    let settings = JsonSerializerSettings()
    settings.Converters.Add(DuConverter())
    settings.NullValueHandling <- NullValueHandling.Ignore
    let result = JsonConvert.SerializeObject(myObj, settings)
    let expectedResult = JsonConvert.SerializeObject(expected)
    Assert.Equal(expectedResult, result)


type NoneObj = {
        prop1: string option
        prop2: string
    }

type ExpectedNonObjResult = {
    prop2: string
}

[<Fact>]
let ``None is ignored`` () =
    //
    let myObj:NoneObj = {
        prop1 = None
        prop2 = "value"
    }
    let expected = {
        prop2 = "value"
    }

    let settings = JsonSerializerSettings()
    settings.Converters.Add(OptionConverter())
    settings.NullValueHandling <- NullValueHandling.Ignore
    let result = JsonConvert.SerializeObject(myObj, settings)
    let expectedResult = JsonConvert.SerializeObject(expected)
    Assert.Equal(expectedResult, result)

type OptionObj = {
        prop1: string option
        prop2: int option
        prop3: float option
        prop4: Kind option
        prop5: string option
    }

type ExpectedOptionObjResult = {
    prop1: string
    prop2: int
    prop3: float
    prop4: string
    prop5: Object
}

[<Fact>]
let ``Can serialize options`` () =
    //
    let myObj:OptionObj = {
        prop1 = Some "test"
        prop2 = Some 10
        prop3 = Some 10.0
        prop4 = Some Kind.BlobStorage
        prop5 = None
    }
    let expected = {
        prop1 = "test"
        prop2 = 10
        prop3 = 10.0
        prop4 = "BlobStorage"
        prop5 = null
    }

    let settings = JsonSerializerSettings()
    settings.Converters.Add(StringEnumConverter())
    settings.Converters.Add(Newtonsoft.Json.Converters.DuConverter())
    settings.Converters.Add(Newtonsoft.Json.Converters.OptionConverter())
    // if not(System.Diagnostics.Debugger.IsAttached) then
    //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
    // while not(System.Diagnostics.Debugger.IsAttached) do
    //   System.Threading.Thread.Sleep(100)
    // System.Diagnostics.Debugger.Break()    
    let result = JsonConvert.SerializeObject(myObj, settings)
    let expectedResult = JsonConvert.SerializeObject(expected)
    Assert.Equal(expectedResult, result)
    // if not(System.Diagnostics.Debugger.IsAttached) then
    //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
    // while not(System.Diagnostics.Debugger.IsAttached) do
    //   System.Threading.Thread.Sleep(100)
    // System.Diagnostics.Debugger.Break()


type TimeSpanObj = {
        prop1: TimeSpan 
        prop2: TimeSpan option
        prop3: TimeSpan option       
    }

type ExpectedTimeSpanObjResult = {
    prop1: string    
    prop2: string
    prop3: Object
}

[<Fact>]
let ``Can serialize timespans`` () =
    let myObj:TimeSpanObj = {
        prop1 = TimeSpan (2,10,5)
        prop2 = Some (TimeSpan (2,10,5))
        prop3 = None
    }
    let expected:ExpectedTimeSpanObjResult = {
        prop1 = "PT2H10M5S"
        prop2 = "PT2H10M5S"
        prop3 = null
    }

    let settings = JsonSerializerSettings()
    settings.Converters.Add(StringEnumConverter())
    settings.Converters.Add(Newtonsoft.Json.Converters.DuConverter())
    settings.Converters.Add(Newtonsoft.Json.Converters.OptionConverter())
    settings.Converters.Add(Newtonsoft.Json.Converters.TimeSpanConverter())
    let result = JsonConvert.SerializeObject(myObj, settings)
    let expectedResult = JsonConvert.SerializeObject(expected)
    Assert.Equal(expectedResult, result)  