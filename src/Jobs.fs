// Note: This is related to the nuget package TimeToFish.
// That is it assumes Azure Service Bus events with one of the following labels:
// Time.Events.MinuteHasPassed, Time.Events.HourHasPassed, Time.Events.DayHasPassed
// Time.Events.MonthHasPassed, Time.Events.YearHasPassed
// The code also assumes that these events are published on a topic named: "time.events"
// The events must have user properties named: Minute, Hour, Day, Month and Year
namespace Bluefin

open Bluefin.Servicebus

module Jobs =
    type IntervalType = Minute | Hour | Day | Month | Year

    let private buildFilterString intervalType interval = 
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
          sprintf "((%s %% %d) = 0)" propName interval
          
        if interval = 0 then typeToLabel else (sprintf "%s AND %s" typeToLabel (buildIntervalFilter interval))

    let createJobSubscriptions rg servicebusNamespace serviceName jobs =

      let createTimeSubscription subscriptionName =
        Subscription.create
                      rg servicebusNamespace "time.events" subscriptionName Subscription.defaultSubscription
      
      let createRule subscriptionName intervalType interval =
                    Rule.createComplete 
                            rg servicebusNamespace "time.events" subscriptionName "timerule" 
                            (Rule.buildSqlRule (buildFilterString intervalType interval))

      let createJob jobDescription =
        let jobName, intervalType, interval = jobDescription
        let subscriptionName = sprintf "%s.%s" serviceName jobName
        createTimeSubscription subscriptionName
        createRule subscriptionName intervalType interval              
        
      jobs |>
      List.iter createJob 
