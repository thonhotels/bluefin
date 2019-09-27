module JobsTests

open Xunit
open System
open Bluefin

[<Fact>]
let ``My test`` () =
    //
    let r = Jobs.buildFilterString Jobs.IntervalType.Minute 10
    Assert.Equal("(sys.Label = 'Time.Events.MinuteHasPassed') AND (((Minute % 10) = 0))", r)

[<Fact>]
let ``My test 55`` () =
    //
    let r = Jobs.buildFilterString Jobs.IntervalType.Minute 55
    Assert.Equal("(sys.Label = 'Time.Events.MinuteHasPassed') AND (Minute > 0 AND ((Minute % 55) = 0))", r)