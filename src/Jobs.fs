// Note: This is related to the nuget package TimeToFish.
// That is it assumes Azure Service Bus events with one of the following labels:
// Time.Events.MinuteHasPassed, Time.Events.HourHasPassed, Time.Events.DayHasPassed
// Time.Events.MonthHasPassed, Time.Events.YearHasPassed
// The code also assumes that these events are published on a topic named: "time.events"
// The events must have user properties named: Minute, Hour, Day, Month and Year
namespace Bluefin

open Bluefin.Servicebus
open System

module Jobs =
    type TimeUnit = Minute | Hour | Day | Month | Year 
    
    type IntervalType = string * TimeUnit * int * TimeSpan

    type SpecificTimeType = string * TimeUnit * int * TimeSpan * TimeSpan

    type Job = Interval of IntervalType | Specific of SpecificTimeType

    let typeToLabel = 
      function 
      | Minute -> "(sys.Label = 'Time.Events.MinuteHasPassed')"
      | Hour -> "(sys.Label = 'Time.Events.HourHasPassed')"        
      | Day -> "(sys.Label = 'Time.Events.DayHasPassed')"
      | Month -> "(sys.Label = 'Time.Events.MonthHasPassed')"
      | Year -> "(sys.Label = 'Time.Events.YearHasPassed')"

    let buildIntervalFilterString timeUnit interval = 
        let buildIntervalFilter interval = 
          if interval <= 0 then failwithf "interval must be a positive number"
          let propName = 
            match timeUnit with
            | Minute -> "Minute"
            | Hour -> "Hour"
            | Day -> "Day"
            | Month -> "Month"
            | Year -> "Year"
         
          let runAt0 = function 
            | Minute when 60 % interval <> 0 -> "Minute > 0 AND "
            | _ -> ""    

          sprintf "(%s((%s %% %d) = 0))" (runAt0 timeUnit) propName interval
      
        if interval = 0 then typeToLabel timeUnit else (sprintf "%s AND %s" (typeToLabel timeUnit)  (buildIntervalFilter interval))

    let buildSpecificTimeFilterString timeUnit time =
      let filterString =
        sprintf "%s = %d" (timeUnit.ToString()) time
      sprintf "%s AND %s" (typeToLabel timeUnit) filterString

    let createJobSubscriptions rg servicebusNamespace serviceName (jobs:seq<Job>) =

      let createTimeSubscription subscriptionName lockDuration timeToLive =
        Subscription.create
                      rg servicebusNamespace "time.events" subscriptionName 
                      { Subscription.defaultSubscription with
                          deadLetteringOnMessageExpiration = false
                          defaultMessageTimeToLive = Some (timeToLive) 
                          lockDuration = lockDuration
                      }
      
      let createIntervalRule subscriptionName timeUnit interval =
                    Rule.createComplete 
                            rg servicebusNamespace "time.events" subscriptionName "timerule" 
                            (Rule.buildSqlRule (buildIntervalFilterString timeUnit interval))

      let createSpecificTimeRule subscriptionName timeUnit time =
                    Rule.createComplete 
                            rg servicebusNamespace "time.events" subscriptionName "timerule" 
                            (Rule.buildSqlRule (buildSpecificTimeFilterString timeUnit time))

      let createJob (job:Job) =
        let intervalTimeToLive tu interval = 
          let divideTimespan (t:TimeSpan) divisor = 
            TimeSpan.FromTicks (t.Ticks / divisor)

          let timespanInterval = 
            match tu with
            | Minute -> TimeSpan (0,0,interval,0)           
            | Hour -> TimeSpan (0,interval,0,0)            
            | Day -> TimeSpan (interval,0,0,0)
            | Month -> TimeSpan (28,0,0,0)
            | Year -> TimeSpan (28,0,0,0)
          min (divideTimespan timespanInterval 2L) (TimeSpan.FromDays 14.0)

        match job with
          | Interval (jobName, timeUnit, interval, lockDuration) -> 
            let subscriptionName = sprintf "%s.%s" serviceName jobName
            createTimeSubscription subscriptionName lockDuration (intervalTimeToLive timeUnit interval)
            createIntervalRule subscriptionName timeUnit interval
          | Specific (jobName, timeUnit, time, lockDuration, timeToLive) -> 
            let subscriptionName = sprintf "%s.%s" serviceName jobName
            createTimeSubscription subscriptionName lockDuration timeToLive
            createSpecificTimeRule subscriptionName timeUnit time

      jobs |>
      Seq.iter createJob 
