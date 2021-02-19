namespace F1ES

module Aggregates =

    open System
    open F1ES.Events

    type Race() =
        member val Id = Guid.Empty with get, set
        member val Title: String option = None with get, set
        member val RaceId: String option = None with get, set
        member val RaceStarted: DateTimeOffset option = None with get, set
        member val RaceEnded: DateTimeOffset option = None with get, set
        member val RedFlaggedTime: DateTimeOffset option = None with get, set
        member val RaceReStarted = Array.empty<DateTimeOffset option> with get, set
        member val Laps = Array.empty<Lap> with get, set
        member val PitLaneOpened = Array.empty<DateTimeOffset option> with get, set
        member val PitLaneClosed = Array.empty<DateTimeOffset option> with get, set
        member val PitLaneOpen: Boolean = true with get, set
        member val Cars = Array.empty<Car> with get, set
        member val ScheduledStartTime: DateTimeOffset option = None with get, set

        member this.Apply(event: RaceScheduled) =
            this.RaceId <- Some(sprintf "%s - %s" event.Country event.Circuit)
            this.Title <- Some event.Title
            ()

        member this.Apply(event: RaceStarted) =
            this.RaceStarted <- Some event.RaceStarted
            ()

        member this.Apply(event: RaceEnded) =
            this.RaceEnded <- Some event.RaceEnded
            ()

        member this.Apply(event: RaceRedFlagged) =
            this.RedFlaggedTime <- Some event.RedFlaggedTime
            ()

        member this.Apply(event: RaceRestarted) =
            this.RaceReStarted <- Array.append this.RaceReStarted [| Some event.RaceRestarted |]
            ()

        member this.Apply(event: PitLaneOpened) =
            this.PitLaneOpened <- Array.append this.PitLaneOpened [| Some event.PitLaneOpened |]
            this.PitLaneOpen <- true
            ()

        member this.Apply(event: PitLaneClosed) =
            this.PitLaneClosed <- Array.append this.PitLaneClosed [| Some event.PitLaneClosed |]
            this.PitLaneOpen <- false
            ()

        member this.Apply(event: RaceDelayed) =
            this.ScheduledStartTime <- Some event.ProposedRaceStartTime
            ()
        
        member this.Apply(event: CarRegistered) =
            this.Cars <- event.Cars
            ()
            
            
    type Driver() =
        member val Id = Guid.Empty with get, set
        member val Name: String = String.Empty with get, set
        
        member this.Apply(event: DriverRegistered) =
          this.Name <- event.Name
          ()
            
            
    