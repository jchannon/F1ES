namespace F1ES

module OutputModel =
    open System
    open F1ES.Events

    //Driver, Car modelled for a race, obviously throughout a season you would want a separate model for each

    type Race() =
        member val Id = Guid.Empty with get, set
        member val RaceId: String option = None with get, set
        member val RaceStarted: DateTimeOffset option = None with get, set
        member val RaceEnded: DateTimeOffset option = None with get, set
        member val RedFlaggedTime: DateTimeOffset option = None with get, set
        member val RaceReStarted = Array.empty<DateTimeOffset option> with get, set
        member val Laps = Array.empty<Lap> with get, set
        member val PitLaneOpened = Array.empty<DateTimeOffset option> with get, set
        member val PitLaneClosed = Array.empty<DateTimeOffset option> with get, set
        member val PitLaneOpen: Boolean = true with get, set
        member val Cars = Array.empty<F1ES.Car> with get, set

        member this.Apply(event: RaceInitialised) =
            this.RaceId <- Some(sprintf "%s - %s" event.Country event.Circuit)

            this.Cars <-
                event.Cars
                |> Array.map (fun x ->
                    { Team = x.Team
                      Driver =
                          { Name = x.DriverName
                            BlackFlagged = false
                            PenaltyApplied = false
                            PenaltyPointsAppied = 0
                            Retired = false
                            Crashed = false }
                      TyreChanged = Array.empty<DateTimeOffset option>
                      NoseChanged = Array.empty<DateTimeOffset option>
                      DownforceChanged = Array.empty<DateTimeOffset option>
                      EnteredPitLane = Array.empty<DateTimeOffset option>
                      ExitedPitLane = Array.empty<DateTimeOffset option> })

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
