namespace F1ES

module Aggregates =

    open System
    open F1ES.Events

    type Race() =
        let updateElement key f array =
            array
            |> Array.map (fun x -> if x.Id = key then f x else x)

        let updateLap key f laps =
            laps
            |> Array.map (fun x -> if x.Number = key then f x else x)

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
        member val CurrentLap = 0 with get, set
        member val SafetyCarOnTrack = false with get, set
        member val VirtualSafetyCarDeployed = false with get, set


        member this.Apply(event: RaceScheduled) =
            this.RaceId <- Some(sprintf "%s - %s" event.Country event.Circuit)
            this.Title <- Some event.Title
            ()

        member this.Apply(event: RaceStarted) =
            this.RaceStarted <- Some event.RaceStarted

            let lap =
                { LapStarted = event.RaceStarted
                  SafetyCarDeployed = None
                  SafetyCarEnded = None
                  VirtualSafetyCarDeployed = None
                  VirtualSafetyCarEnded = None
                  Number = this.Laps.Length + 1 }

            this.Laps <- Array.append this.Laps [| lap |]
            this.CurrentLap <- 1
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

        member this.Apply(event: CarEnteredPitLane) =
            this.Cars <-
                this.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             EnteredPitLane = Array.append car.EnteredPitLane [| event.EntryTime |]
                             InPitLane = true })

            ()

        member this.Apply(event: CarExitedPitLane) =
            this.Cars <-
                this.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             ExitedPitLane = Array.append car.ExitedPitLane [| event.ExitTime |]
                             InPitLane = false })

            ()

        member this.Apply(event: CarEnteredPitBox) =
            this.Cars <-
                this.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             EnteredPitBox = Array.append car.EnteredPitBox [| event.EntryTime |]
                             InPitBox = true })

            ()

        member this.Apply(event: CarExitedPitBox) =
            this.Cars <-
                this.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             ExitedPitBox = Array.append car.ExitedPitBox [| event.ExitTime |]
                             InPitBox = false })

            ()

        member this.Apply(event: LapStarted) =
            let lap =
                { LapStarted = event.LapStartedTime
                  SafetyCarDeployed = None
                  SafetyCarEnded = None
                  VirtualSafetyCarDeployed = None
                  VirtualSafetyCarEnded = None
                  Number = this.Laps.Length + 1 }

            this.Laps <- Array.append this.Laps [| lap |]
            this.CurrentLap <- lap.Number

        member this.Apply(event: SafetyCarDeployed) =
            this.Laps <-
                this.Laps
                |> updateLap event.CurrentLap (fun lap ->
                       { lap with
                             SafetyCarDeployed = Some event.DeployedTime })

            this.SafetyCarOnTrack <- true
            ()

        member this.Apply(event: SafetyCarRecalled) =
            this.Laps <-
                this.Laps
                |> updateLap event.CurrentLap (fun lap ->
                       { lap with
                             SafetyCarEnded = Some event.RecallTime })

            this.SafetyCarOnTrack <- false
            ()

        member this.Apply(event: VirtualSafetyCarDeployed) =
            this.Laps <-
                this.Laps
                |> updateLap event.CurrentLap (fun lap ->
                       { lap with
                             VirtualSafetyCarDeployed = Some event.DeployedTime })

            this.VirtualSafetyCarDeployed <- true
            ()

        member this.Apply(event: VirtualSafetyCarRecalled) =
            this.Laps <-
                this.Laps
                |> updateLap event.CurrentLap (fun lap ->
                       { lap with
                             VirtualSafetyCarEnded = Some event.RecallTime })

            this.VirtualSafetyCarDeployed <- false
            ()

        member this.Apply(event: TyreChanged) =
            this.Cars <-
                this.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             TyreChanged = Array.append car.TyreChanged [| event.TyreChangedTime |] })

        member this.Apply(event: NoseChanged) =
            this.Cars <-
                this.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             NoseChanged = Array.append car.TyreChanged [| event.NoseChangedTime |] })
                
        member this.Apply(event: DownforceChanged) =
            this.Cars <-
                this.Cars
                |> updateElement event.CarId (fun car ->
                       { car with
                             DownforceChanged = Array.append car.TyreChanged [| event.DownforceChangedTime |] })



    type Driver() =
        member val Id = Guid.Empty with get, set
        member val Name: String = String.Empty with get, set

        member this.Apply(event: DriverRegistered) =
            this.Name <- event.Name
            ()
