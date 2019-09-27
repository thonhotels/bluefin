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
    type IntervalType = Minute | Hour | Day | Month | Year

    let buildFilterString intervalType interval = 
        let typeToLabel = 
          match intervalType with
          | Minute -> "(sys.Label = 'Time.Events.MinuteHasPassed')"
          | Hour -> "(sys.Label = 'Time.Events.HourHasPassed')"
          | Day -> "(sys.Label = 'Time.Events.DayHasPassed')"
          | Month -> "(sys.Label = 'Time.Events.MonthHasPassed')"
          | Year -> "(sys.Label = 'Time.Events.YearHasPassed')"

        let buildIntervalFilter interval = 
          if interval <= 0 then failwithf "interval must be a positive number"
          let propName = 
            match intervalType with
            | Minute -> "Minute"
            | Hour -> "Hour"
            | Day -> "Day"
            | Month -> "Month"
            | Year -> "Year"
         
          let runAt0 = function 
            | Minute when 60 % interval <> 0 -> "Minute > 0 AND "
            | _ -> ""    

          sprintf "(%s((%s %% %d) = 0))" (runAt0 intervalType) propName interval

        if interval = 0 then typeToLabel else (sprintf "%s AND %s" typeToLabel (buildIntervalFilter interval))

    let createJobSubscriptions rg servicebusNamespace serviceName jobs =

      let createTimeSubscription subscriptionName intervalType interval lockDuration =
        let divideTimespan (t:TimeSpan) divisor = 
          TimeSpan.FromTicks (t.Ticks / divisor)

          
        let timeToLive = 
          let timespanInterval = 
            match intervalType with
            | Minute -> TimeSpan (0,0,interval,0)           
            | Hour -> TimeSpan (0,interval,0,0)
            | Day -> TimeSpan (interval,0,0,0)
            | Month -> TimeSpan (28,0,0,0)
            | Year -> TimeSpan (28,0,0,0)
          min (divideTimespan timespanInterval 2L) (TimeSpan.FromDays 14.0)
          
        Subscription.create
                      rg servicebusNamespace "time.events" subscriptionName 
                      { Subscription.defaultSubscription with
                          deadLetteringOnMessageExpiration = false
                          defaultMessageTimeToLive = Some (timeToLive) 
                          lockDuration = lockDuration
                      }
      
      let createRule subscriptionName intervalType interval =
                    Rule.createComplete 
                            rg servicebusNamespace "time.events" subscriptionName "timerule" 
                            (Rule.buildSqlRule (buildFilterString intervalType interval))

      let createJob jobDescription =
        let jobName, intervalType, interval, lockDuration = jobDescription
        let subscriptionName = sprintf "%s.%s" serviceName jobName
        createTimeSubscription subscriptionName intervalType interval lockDuration
        createRule subscriptionName intervalType interval              
        
      jobs |>
      List.iter createJob 
