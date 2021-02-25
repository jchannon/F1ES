namespace F1ES

open System

type Lap =
        { LapStarted: DateTimeOffset
          Number: int
          SafetyCarDeployed: DateTimeOffset option
          SafetyCarEnded: DateTimeOffset option
          VirtualSafetyCarDeployed: DateTimeOffset option
          VirtualSafetyCarEnded: DateTimeOffset option }