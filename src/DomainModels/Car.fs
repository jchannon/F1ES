namespace F1ES

open System

type Car =
    { Team: Team
      Driver: Driver
      TyreChanged: DateTimeOffset option []
      NoseChanged: DateTimeOffset option []
      DownforceChanged: DateTimeOffset option []
      mutable EnteredPitLane: DateTimeOffset []
      ExitedPitLane: DateTimeOffset option []
      Id : Guid}
