namespace F1ES

open System

module Events =
    type RaceStarted(id:Guid,raceStarted:DateTimeOffset) =
        member this.RaceStarted = raceStarted
        member this.RaceId = id
        
    type RaceEnded(raceEnded:DateTimeOffset) =
        member this.RaceEnded = raceEnded
