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
            
            //self.DeleteEvent<RaceStarted>() |> ignore

        member this.ApplyRaceScheduled (projection: RaceSummary) (event: RaceScheduled) =
            projection.Circuit <- event.Circuit
            projection.Country <- event.Country
            projection.RaceId <- sprintf "%s - %s" event.Country event.Circuit

            projection.Cars <- event.Cars
                
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