namespace F1ES

open System

type Driver =
        { DriverId: Guid
          BlackFlagged: bool
          PenaltyApplied: bool
          PenaltyPointsApplied: int
          Retired: bool
          Crashed: bool }
