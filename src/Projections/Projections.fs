namespace F1ES

module Projections =

    open System
    open System.Text.Json.Serialization
    open F1ES.Events
    open Marten.Events.Projections

    type RaceSummary() =
        member val Id = Guid.Empty with get, set
        member val Title: String = "" with get, set
        member val RaceId: String = "" with get, set
        member val RaceStarted: DateTimeOffset option = None with get, set
        member val RaceEnded: DateTimeOffset option = None with get, set
        member val RedFlaggedTime: DateTimeOffset option = None with get, set
        member val RaceReStarted = Array.empty<DateTimeOffset option> with get, set
        member val Laps = Array.empty<Lap> with get, set
        member val PitLaneOpened = Array.empty<DateTimeOffset option> with get, set
        member val PitLaneClosed = Array.empty<DateTimeOffset option> with get, set
        member val PitLaneOpen: Boolean = true with get, set
        member val Cars = Array.empty<Car> with get, set
        member val Circuit: string = "" with get, set
        member val Country: string = "" with get, set
        member val ScheduledStartTime: DateTimeOffset = DateTimeOffset.MinValue with get, set
        member val CurrentLap = 0 with get, set
        member val SafetyCarOnTrack = false with get,set

    type RaceProjection() as self =
        inherit ViewProjection<RaceSummary, Guid>()

        let updateElement key f array =
            array
            |> Array.map (fun x -> if x.Id = key then f x else x)
            
        let updateLap key f laps =
            laps
            |> Array.map (fun x -> if x.Number = key then f x else x)

        do
            self.ProjectEvent<RaceScheduled>(self.ApplyRaceScheduled)
            |> ignore

            self.ProjectEvent<RaceStarted>(self.ApplyRaceStarted)
            |> ignore

            self.ProjectEvent<RaceEnded>(self.ApplyRaceEnded)
            |> ignore

            self.ProjectEvent<RaceRedFlagged>(self.ApplyRaceRedFlagged)
            |> ignore

            self.ProjectEvent<RaceRestarted>(self.ApplyRaceRestarted)
            |> ignore

            self.ProjectEvent<PitLaneOpened>(self.ApplyPitLaneOpened)
            |> ignore

            self.ProjectEvent<PitLaneClosed>(self.ApplyPitLaneClosed)
            |> ignore

            self.ProjectEvent<RaceDelayed>(self.ApplyRaceDelayed)
            |> ignore

            self.ProjectEvent<CarRegistered>(self.ApplyCarRegistered)
            |> ignore

            self.ProjectEvent<CarEnteredPitLane>(self.ApplyCarEnteredPitLane)
            |> ignore

            self.ProjectEvent<CarExitedPitLane>(self.ApplyCarExitedPitLane)
            |> ignore

            self.ProjectEvent<CarEnteredPitBox>(self.ApplyCarEnteredPitBox)
            |> ignore

            self.ProjectEvent<CarExitedPitBox>(self.ApplyCarExitedPitBox)
            |> ignore
            
            self.ProjectEvent<LapStarted>(self.ApplyLapStarted)
            |> ignore
            
            self.ProjectEvent<SafetyCarDeployed>(self.ApplySafetyCarDeployed)
            |> ignore

        //self.DeleteEvent<RaceStarted>() |> ignore

        member this.ApplyRaceScheduled (projection: RaceSummary) (event: RaceScheduled) =
            projection.Circuit <- event.Circuit
            projection.Country <- event.Country
            projection.RaceId <- sprintf "%s - %s" event.Country event.Circuit
            projection.Title <- event.Title
            projection.ScheduledStartTime <- event.ScheduledStartTime

            ()

        member this.ApplyRaceStarted (projection: RaceSummary) (event: RaceStarted) =
            projection.RaceStarted <- Some event.RaceStarted
            let lap =
                { LapStarted = event.RaceStarted
                  SafetyCarDeployed = None
                  SafetyCarEnded = None
                  VirtualSafetyCarDeployed = None
                  VirtualSafetyCarEnded = None
                  Number = projection.Laps.Length + 1 }

            projection.Laps <- Array.append projection.Laps [| lap |]
            projection.CurrentLap <- 1
            ()

        member this.ApplyRaceEnded (projection: RaceSummary) (event: RaceEnded) =
            projection.RaceEnded <- Some event.RaceEnded
            ()

        member this.ApplyRaceRedFlagged (projection: RaceSummary) (event: RaceRedFlagged) =
            projection.RedFlaggedTime <- Some event.RedFlaggedTime
            ()

        member this.ApplyRaceRestarted (projection: RaceSummary) (event: RaceRestarted) =
            projection.RaceReStarted <- Array.append projection.RaceReStarted [| Some event.RaceRestarted |]
            ()

        member this.ApplyPitLaneOpened (projection: RaceSummary) (event: PitLaneOpened) =
            projection.PitLaneOpened <- Array.append projection.PitLaneOpened [| Some event.PitLaneOpened |]
            projection.PitLaneOpen <- true
            ()

        member this.ApplyPitLaneClosed (projection: RaceSummary) (event: PitLaneClosed) =
            projection.PitLaneClosed <- Array.append projection.PitLaneClosed [| Some event.PitLaneClosed |]
            projection.PitLaneOpen <- false
            ()

        member this.ApplyRaceDelayed (projection: RaceSummary) (event: RaceDelayed) =
            projection.ScheduledStartTime <- event.ProposedRaceStartTime
            ()

        member this.ApplyCarRegistered (projection: RaceSummary) (event: CarRegistered) =
            projection.Cars <- event.Cars
            ()

        member this.ApplyCarEnteredPitLane (projection: RaceSummary) (event: CarEnteredPitLane) =
            projection.Cars <-
                projection.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             EnteredPitLane = Array.append car.EnteredPitLane [| event.EntryTime |]
                             InPitLane = true })

            ()

        member this.ApplyCarExitedPitLane (projection: RaceSummary) (event: CarExitedPitLane) =
            projection.Cars <-
                projection.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             ExitedPitLane = Array.append car.ExitedPitLane [| event.ExitTime |]
                             InPitLane = false })

            ()

        member this.ApplyCarEnteredPitBox (projection: RaceSummary) (event: CarEnteredPitBox) =
            projection.Cars <-
                projection.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             EnteredPitBox = Array.append car.EnteredPitBox [| event.EntryTime |]
                             InPitBox = true })

            ()

        member this.ApplyCarExitedPitBox (projection: RaceSummary) (event: CarExitedPitBox) =
            projection.Cars <-
                projection.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             ExitedPitBox = Array.append car.ExitedPitBox [| event.ExitTime |]
                             InPitBox = false })

            ()
            
        member this.ApplyLapStarted (projection:RaceSummary) (event: LapStarted) =
            let lap =
                { LapStarted = event.LapStartedTime
                  SafetyCarDeployed = None
                  SafetyCarEnded = None
                  VirtualSafetyCarDeployed = None
                  VirtualSafetyCarEnded = None
                  Number = projection.Laps.Length + 1 }

            projection.Laps <- Array.append projection.Laps [| lap |]
            
        member this.ApplySafetyCarDeployed (projection:RaceSummary) (event: SafetyCarDeployed) =
            projection.Laps <-
                projection.Laps
                |> updateLap event.CurrentLap (fun lap ->
                       { lap with
                             SafetyCarDeployed = Some event.DeployedTime })
                
            projection.SafetyCarOnTrack <- true

            ()

    type Pitstop =
        { PitLaneEntryTime: DateTimeOffset
          PitLaneExitTime: DateTimeOffset
          PitBoxEntryTime: DateTimeOffset
          PitBoxExitTime: DateTimeOffset }
        member this.PitBoxTime =
            this.PitBoxExitTime - this.PitBoxEntryTime

        member this.PitLaneTime =
            this.PitLaneExitTime - this.PitLaneEntryTime

    type PitstopSummary() =
        member val Id = Guid.Empty with get, set
        member val CarId = Guid.Empty with get, set
        member val PitStops = Array.empty<Pitstop> with get, set

    type PitstopProjection() as self =
        inherit ViewProjection<PitstopSummary, Guid>()

        let updateElement key f array =
            array
            |> Array.map (fun x -> if x.PitLaneEntryTime = key then f x else x)

        do
            self.ProjectEvent<CarEnteredPitLane>(self.ApplyCarEnteredPitLane)
            |> ignore

            self.ProjectEvent<CarExitedPitLane>(self.ApplyCarExitedPitLane)
            |> ignore

            self.ProjectEvent<CarEnteredPitBox>(self.ApplyCarEnteredPitBox)
            |> ignore

            self.ProjectEvent<CarExitedPitBox>(self.ApplyCarExitedPitBox)
            |> ignore

        member this.ApplyCarEnteredPitLane (projection: PitstopSummary) (event: CarEnteredPitLane) =
            projection.CarId <- event.CarId

            projection.PitStops <-
                Array.append
                    projection.PitStops
                    [| { PitLaneEntryTime = event.EntryTime
                         PitLaneExitTime = DateTimeOffset.MinValue
                         PitBoxEntryTime = DateTimeOffset.MinValue
                         PitBoxExitTime = DateTimeOffset.MinValue } |]

            ()

        member this.ApplyCarExitedPitLane (projection: PitstopSummary) (event: CarExitedPitLane) =
            projection.CarId <- event.CarId

            let latestPitstop =
                projection.PitStops
                |> Array.sortByDescending (fun x -> x.PitLaneEntryTime)
                |> Array.head

            projection.PitStops <-
                projection.PitStops
                |> updateElement latestPitstop.PitLaneEntryTime (fun pitstop ->
                       { pitstop with
                             PitLaneExitTime = event.ExitTime })

            ()

        member this.ApplyCarEnteredPitBox (projection: PitstopSummary) (event: CarEnteredPitBox) =
            projection.CarId <- event.CarId

            let latestPitstop =
                projection.PitStops
                |> Array.sortByDescending (fun x -> x.PitLaneEntryTime)
                |> Array.head

            projection.PitStops <-
                projection.PitStops
                |> updateElement latestPitstop.PitLaneEntryTime (fun pitstop ->
                       { pitstop with
                             PitBoxEntryTime = event.EntryTime })

            ()

        member this.ApplyCarExitedPitBox (projection: PitstopSummary) (event: CarExitedPitBox) =
            projection.CarId <- event.CarId

            let latestPitstop =
                projection.PitStops
                |> Array.sortByDescending (fun x -> x.PitLaneEntryTime)
                |> Array.head

            projection.PitStops <-
                projection.PitStops
                |> updateElement latestPitstop.PitLaneEntryTime (fun pitstop ->
                       { pitstop with
                             PitBoxExitTime = event.ExitTime })

            ()
