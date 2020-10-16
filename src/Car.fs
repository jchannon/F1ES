namespace F1ES

open System

type Car =
    { Team: Team
      Driver: Driver
      TyreChanged: DateTimeOffset option []
      NoseChanged: DateTimeOffset option []
      DownforceChanged: DateTimeOffset option []
      EnteredPitLane: DateTimeOffset option []
      ExitedPitLane: DateTimeOffset option [] }
