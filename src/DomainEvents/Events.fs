namespace F1ES

module Events =
    open System

    type Car = {DriverName:string; Team:Team}
    
    type RaceScheduled(country:string,circuit:string,cars:Car[],title:string) =
        member this.Cars = cars
        member this.Country = country
        member this.Circuit = circuit
        member this.Title = title
        
        
    type RaceStarted(raceStarted:DateTimeOffset) =
        member this.RaceStarted = raceStarted
        
    type RaceEnded(raceEnded:DateTimeOffset) =
        member this.RaceEnded = raceEnded

    type RaceRedFlagged(redFlaggedTime:DateTimeOffset) =
        member this.RedFlaggedTime = redFlaggedTime
    
    type RaceRestarted(raceRestarted:DateTimeOffset) =
        member this.RaceRestarted = raceRestarted
        
    type PitLaneOpened(pitLaneOpened:DateTimeOffset) =
        member this.PitLaneOpened = pitLaneOpened
        
    type PitLaneClosed(pitLaneClosed:DateTimeOffset) =
        member this.PitLaneClosed = pitLaneClosed
        
    type RaceDelayed(proposedRaceStartTime:DateTimeOffset)=
        member this.ProposedRaceStartTime = proposedRaceStartTime