namespace F1ES

module Model=
    open System

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
          Retired:bool
          Crashed:bool }

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

    type Race =
        { RaceStarted: DateTimeOffset
          RaceEnded: DateTimeOffset
          RaceRedFlagged: bool
          RaceRedFlaggedTime: DateTimeOffset
          RaceRestarted: DateTimeOffset []
          Laps: Lap []
          PitLaneOpened: DateTimeOffset
          PitLaneClosed: DateTimeOffset
          Cars: Car [] }