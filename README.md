# F1ES

A HTTP/CQRS/ES system based on a Formula 1 domain.  

The system acts as if a race in a season can be managed. A driver can be registered outside of the context of a race.
A race can be scheduled for a location, circuit and date for example.  Cars for a team and a driver id can be registered for the race.
The race can be started, re-started, red flagged, re-scheduled, pit lane opened/closed. A car can enter a pit lane and a pit box and also exit these.

These events can be triggered via HTTP. They are then recorded in a Marten event store. Race information can be queried also using Marten projections
## HTTP

## CQRS

## ES

### Events
 
  
DriverBlackFlagged  
DriverEngagedDRS
DriverDisEngagedDRS 
DriverCrashed  
DriverRetired  
 


### Aggregates

## References

1. F1 2020 Rules https://www.statsf1.com/reglement/sportif.pdf
