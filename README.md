# F1ES

A HTTP/CQRS/ES system based on a Formula 1 domain.  This system aims to illustrate each architectural component as an example of how to implement each one in a project.

## HTTP

## CQRS

## ES

### Events

RaceStarted  
RaceRedFlagged  
RaceRestarted  
DriverEnteredPitLane  
DriverEnteredPitBox  
DriverExitedPitBox  
DriverExitedPitLane   
DriverPenaltyApplied  
DriverBlackFlagged  
DriverEngagedDRS
DriverDisEngagedDRS
TyreChanged  
NoseChanged  
DownforceChanged  
LapStarted  
LapEnded  
DriverCrashed  
DriverRetired  
SafetyCarDeployed  
SafetyCarEnded    
VirtualSafetyCarDeployed  
VirtualSafetyCarEnded  
PitLaneOpened
PitLandClosed - can be closed for safety reasons see Hamilton 2020 entering when closed, the exit is closed 30mins before the race starts
RaceEnded  

### Aggregates

## References

1. F1 2020 Rules https://www.statsf1.com/reglement/sportif.pdf
