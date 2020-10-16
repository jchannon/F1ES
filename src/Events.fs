namespace F1ES

open System

module Events =
    type Car = {DriverName:string; Team:Team}
    
    type RaceInitialised(country:string,circuit:string,cars:Car[]) =
        member this.Cars = cars
        member this.Country = country
        member this.Circuit = circuit
        
        
    type RaceStarted(raceStarted:DateTimeOffset) =
        member this.RaceStarted = raceStarted
        
    type RaceEnded(raceEnded:DateTimeOffset) =
        member this.RaceEnded = raceEnded
