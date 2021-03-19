namespace F1ES

open System

type Driver =
        { DriverId: Guid
          BlackFlagged: bool
          PenaltyPoints: int
          DriveThroughPenaltyInSeconds:int
          Retired: bool }
