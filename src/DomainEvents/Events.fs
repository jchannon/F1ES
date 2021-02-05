namespace F1ES

module Events =
    open System

    type Car = {DriverName:string; Team:Team}
    
    type RaceInitialised(country:string,circuit:string,cars:Car[]) =
        member this.Cars = cars
        member this.Country = country
        member this.Circuit = circuit
        
        
    type RaceStarted(raceStarted:DateTimeOffset) =
        member this.RaceStarted = raceStarted
        
    type RaceEnded(raceEnded:DateTimeOffset) =
        member this.RaceEnded = raceEnded

    type RaceRedFlagged(redFlaggedTime:DateTimeOffset) =
        member this.RedFlaggedTime = redFlaggedTime
    
    type RaceRestarted(raceRestarted:DateTimeOffset) =
        member this.RaceRestarted = raceRestarted