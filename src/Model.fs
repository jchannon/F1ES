namespace F1ES

module Model =
    open System
    open F1ES.Events
    //Driver, Car modelled for a race, obviously throughout a season you would want a separate model for each

    type Team =
        | Mercedes
        | Ferrari
        | RedBull
        | McLaren
        | RacingPoint
        | Renault
        | AlphaTauri
        | AlphaRomeoRacing
        | Haas
        | Williams

    type Driver =
        { Name: string
          BlackFlagged: bool
          PenaltyApplied: bool
          PenaltyPointsAppied: int
          Retired: bool
          Crashed: bool }

    type Car =
        { Team: Team
          Driver: Driver
          TyreChanged: DateTimeOffset []
          NoseChanged: DateTimeOffset []
          DownforceChanged: DateTimeOffset []
          EnteredPitLane: DateTimeOffset []
          ExitedPitLane: DateTimeOffset [] }

    type Lap =
        { LapStarted: DateTimeOffset
          LapEnded: DateTimeOffset
          Number: int
          SafetyCarDeployed: DateTimeOffset option
          SafetyCarEnded: DateTimeOffset option
          VirtualSafetyCarDeployed: DateTimeOffset option
          VirtualSafetyCarEnded: DateTimeOffset option }

    type Race() =

        member this.Apply(event: RaceStarted) =
            this.RaceStarted <- Some event.RaceStarted
            this.Id <- event.RaceId
            printfn "%A" this.Id
            ()

        member val Id = Guid.Empty with get, set
        member val RaceStarted: DateTimeOffset option = None with get, set
        member val RaceEnded: DateTimeOffset option = None with get, set
        member val RedFlagged = false with get, set
        member val RedFlaggedTime: DateTimeOffset option = None with get, set
        member val RaceReStarted = Array.empty<DateTimeOffset option> with get, set
        member val Laps = Array.empty<Lap> with get, set
        member val PitLaneOpened: DateTimeOffset option = None with get, set
        member val PitLaneClosed: DateTimeOffset option = None with get, set
        member val Cars = Array.empty<Car> with get, set
        
        member this.Apply(event: RaceEnded) =
            this.RaceStarted <- Some event.RaceEnded
            printfn "%A" this.Id
            ()
