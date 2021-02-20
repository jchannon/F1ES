namespace F1ES

open System

type Car =
    { Team: Team
      Driver: Driver
      TyreChanged: DateTimeOffset option []
      NoseChanged: DateTimeOffset option []
      DownforceChanged: DateTimeOffset option []
      InPitLane:bool
      EnteredPitLane: DateTimeOffset []
      ExitedPitLane: DateTimeOffset []
      InPitBox:bool
      EnteredPitBox: DateTimeOffset []
      ExitedPitBox: DateTimeOffset []
      Id : Guid}
