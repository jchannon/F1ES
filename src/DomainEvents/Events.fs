namespace F1ES

module Events =
    open System


    type RaceScheduled(country: string, circuit: string, title: string, scheduledStartTime: DateTimeOffset) =
        member this.Country = country
        member this.Circuit = circuit
        member this.Title = title
        member this.ScheduledStartTime = scheduledStartTime


    type RaceStarted(raceStarted: DateTimeOffset) =
        member this.RaceStarted = raceStarted

    type RaceEnded(raceEnded: DateTimeOffset) =
        member this.RaceEnded = raceEnded

    type RaceRedFlagged(redFlaggedTime: DateTimeOffset) =
        member this.RedFlaggedTime = redFlaggedTime

    type RaceRestarted(raceRestarted: DateTimeOffset) =
        member this.RaceRestarted = raceRestarted

    type PitLaneOpened(pitLaneOpened: DateTimeOffset) =
        member this.PitLaneOpened = pitLaneOpened

    type PitLaneClosed(pitLaneClosed: DateTimeOffset) =
        member this.PitLaneClosed = pitLaneClosed

    type RaceDelayed(proposedRaceStartTime: DateTimeOffset) =
        member this.ProposedRaceStartTime = proposedRaceStartTime

    type CarsRegistered(cars: Car []) =
        member this.Cars = cars

    type CarEnteredPitLane(carId: Guid, entryTime: DateTimeOffset) =
        member this.CarId = carId
        member this.EntryTime = entryTime

    type CarExitedPitLane(carId: Guid, exitTime: DateTimeOffset) =
        member this.CarId = carId
        member this.ExitTime = exitTime
        
    type CarEnteredPitBox(carId: Guid, entryTime: DateTimeOffset) =
        member this.CarId = carId
        member this.EntryTime = entryTime

    type CarExitedPitBox(carId: Guid, exitTime: DateTimeOffset) =
        member this.CarId = carId
        member this.ExitTime = exitTime

    type DriverRegistered(name: string) =
        member this.Name = name
        
    type LapStarted(lapStartedTime:DateTimeOffset) =
        member this.LapStartedTime = lapStartedTime
        
    type SafetyCarDeployed(deployedTime:DateTimeOffset, currentLap:int) =
        member this.DeployedTime = deployedTime
        member this.CurrentLap = currentLap
        
    type SafetyCarRecalled(recallTime:DateTimeOffset, currentLap:int) =
        member this.RecallTime = recallTime
        member this.CurrentLap = currentLap
        
    type VirtualSafetyCarDeployed(deployedTime:DateTimeOffset, currentLap:int) =
        member this.DeployedTime = deployedTime
        member this.CurrentLap = currentLap
        
    type VirtualSafetyCarRecalled(recallTime:DateTimeOffset, currentLap:int) =
        member this.RecallTime = recallTime
        member this.CurrentLap = currentLap
        
    type TyreChanged(carId: Guid, tyreChangedTime: DateTimeOffset) =
        member this.CarId = carId
        member this.TyreChangedTime = tyreChangedTime
        
    type NoseChanged(carId: Guid, noseChangedTime: DateTimeOffset) =
        member this.CarId = carId
        member this.NoseChangedTime = noseChangedTime
        
    type DownforceChanged(carId: Guid, downforceChangedTime: DateTimeOffset) =
        member this.CarId = carId
        member this.DownforceChangedTime = downforceChangedTime
        
    type PenaltyPointsApplied(carId: Guid, penaltyPoints: int) =
        member this.CarId = carId
        member this.PenaltyPoints = penaltyPoints
        
    type DriveThroughPenaltyApplied(carId: Guid, penaltyPoints: int) =
        member this.CarId = carId
        member this.DriveThroughPenaltyInSeconds = penaltyPoints
        
    type DriverRetired(carId: Guid) =
        member this.CarId = carId
        
    type DriverBlackFlagged(carId: Guid) =
        member this.CarId = carId
