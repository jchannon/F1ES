namespace F1ES

open System

type Car =
    { Team: Team
      Driver: Driver
      TyreChanged: DateTimeOffset  []
      NoseChanged: DateTimeOffset  []
      DownforceChanged: DateTimeOffset  []
      InPitLane:bool
      EnteredPitLane: DateTimeOffset []
      ExitedPitLane: DateTimeOffset []
      InPitBox:bool
      EnteredPitBox: DateTimeOffset []
      ExitedPitBox: DateTimeOffset []
      Crashes: DateTimeOffset []
      Id : Guid}
