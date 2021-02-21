namespace F1ES

module Projections =

    open System
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


    type RaceProjection() as self =
        inherit ViewProjection<RaceSummary, Guid>()

        let updateElement key f array =
            array
            |> Array.map (fun x -> if x.Id = key then f x else x)

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

    type PitstopSummary() =
        member val Id = Guid.Empty with get, set
        member val PitLaneEntryTime: DateTimeOffset = DateTimeOffset.MinValue with get, set
        member val PitLaneExitTime: DateTimeOffset = DateTimeOffset.MinValue with get, set

        member this.PitLaneTime: TimeSpan =
            this.PitLaneExitTime - this.PitLaneEntryTime

        member val PitBoxEntryTime: DateTimeOffset = DateTimeOffset.MinValue with get, set
        member val PitBoxExitTime: DateTimeOffset = DateTimeOffset.MinValue with get, set

        member this.PitBoxTime: TimeSpan =
            this.PitBoxExitTime - this.PitBoxEntryTime


    type PitstopProjection() as self =
        inherit ViewProjection<PitstopSummary, Guid>()

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
            projection.Id <- event.CarId
            projection.PitLaneEntryTime <- event.EntryTime
            ()

        member this.ApplyCarExitedPitLane (projection: PitstopSummary) (event: CarExitedPitLane) =
            projection.Id <- event.CarId
            projection.PitLaneExitTime <- event.ExitTime
            ()

        member this.ApplyCarEnteredPitBox (projection: PitstopSummary) (event: CarEnteredPitBox) =
            projection.Id <- event.CarId
            projection.PitBoxEntryTime <- event.EntryTime

            ()

        member this.ApplyCarExitedPitBox (projection: PitstopSummary) (event: CarExitedPitBox) =
            projection.Id <- event.CarId
            projection.PitBoxExitTime <- event.ExitTime

            ()
