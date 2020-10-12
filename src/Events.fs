namespace F1ES

open System

module Events =
    type RaceStarted(raceId:Guid,raceStarted:DateTimeOffset) =
        member this.RaceStarted = raceStarted
        member this.RaceId = raceId
        
    type RaceEnded(raceEnded:DateTimeOffset) =
        member this.RaceEnded = raceEnded
