namespace F1ES

module CommandHandlers =

    open System
    open F1ES.Events
    open Marten
    open F1ES.InputModels
    open F1ES.ProblemDetails
    open F1ES.Aggregates
    open F1ES.Projections
    open System.Linq
    open Marten.Exceptions
    open F1ES.Constants

    let handleRaceScheduled (store: IDocumentStore) (message: RaceScheduledInput) path =

        use session = store.OpenSession()

        try
            let stream = session.Events.StartStream<Race>()

            let raceScheduled =
                RaceScheduled(message.Country, message.TrackName, message.Title, message.ScheduledStartTime)

            session.Events.Append(stream.Id, raceScheduled)
            |> ignore

            session.SaveChanges()

            Ok stream.Id
        with :? MartenCommandException ->
            Error
                { Detail = "Please ensure you enter unique details for the race"
                  Status = 422
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

    let getRace (store: IDocumentStore) (streamId: Guid) =
        use session = store.OpenSession()

        session
            .Query<RaceSummary>()
            .Where(fun x -> x.Id = streamId)
            .Single()

    let startRace (store: IDocumentStore) (streamId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(streamId)

        match race.RaceStarted, race.RaceEnded, race.Cars.Length with
        | Some _, Some _, carCount
        | None, Some _, carCount ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _, carCount ->
            //TODO Create proper URI for problem details
            Error
                { Detail = "The race has already started"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

        | None, None, carCount when carCount >= 1 ->
            let raceStarted = RaceStarted(DateTimeOffset.UtcNow)
            let pitLaneOpened = PitLaneOpened(DateTimeOffset.UtcNow)


            session.Events.Append(streamId, raceStarted, pitLaneOpened)
            |> ignore

            session.SaveChanges()
            Ok()
        | None, None, carCount ->
            Error
                { Detail = "Not enough cars registered to start the race"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

    let stopRace (store: IDocumentStore) (streamId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(streamId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None _, None ->
            Error
                { Detail = "The race has not started"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, Some _ ->
            Error
                { Detail = "The race has not started but ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None ->
            let raceEnded = RaceEnded(DateTimeOffset.UtcNow)

            session.Events.Append(streamId, raceEnded)
            |> ignore

            session.SaveChanges()
            Ok()

    //TODO Race's have to be stopped after 2 hours

    let restartRace (store: IDocumentStore) (streamId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(streamId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _ ->
            Error
                { Detail = "The race has ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race has not started"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, Some _ ->
            Error
                { Detail = "The race has not started but has ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None ->
            let raceRestarted = RaceRestarted(DateTimeOffset.UtcNow)

            session.Events.Append(streamId, raceRestarted)
            |> ignore

            session.SaveChanges()
            Ok()

    let redflagRace (store: IDocumentStore) (streamId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(streamId)

        match race.RedFlaggedTime, race.RaceStarted, race.RaceEnded with

        | None, Some _, Some _
        | None, None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

        | Some _, None, Some _
        | Some _, None, None
        | Some _, Some _, None
        | Some _, Some _, Some _ ->
            Error
                { Detail = "The race has already been red flagged"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

        | None, None, None
        | None, Some _, None ->
            let raceRedFlagged = RaceRedFlagged(DateTimeOffset.UtcNow)
            let raceEnded = RaceEnded(DateTimeOffset.UtcNow)

            session.Events.Append(streamId, raceRedFlagged, raceEnded)
            |> ignore

            session.SaveChanges()
            Ok()

    let openPitLane (store: IDocumentStore) (streamId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(streamId)

        match race.PitLaneOpen with
        | false ->

            let pitLaneOpened = PitLaneOpened(DateTimeOffset.UtcNow)

            session.Events.Append(streamId, pitLaneOpened)
            |> ignore

            session.SaveChanges()
            Ok()
        | true ->
            Error
                { Detail = "The pitlane is already opened"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

    let closePitLane (store: IDocumentStore) (streamId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(streamId)

        match race.PitLaneOpen with
        | true ->

            let pitLaneClosed = PitLaneClosed(DateTimeOffset.UtcNow)

            session.Events.Append(streamId, pitLaneClosed)
            |> ignore

            session.SaveChanges()
            Ok()
        | false ->
            Error
                { Detail = "The pitlane is already closed"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

    let delayStartTime (store: IDocumentStore) (streamId: Guid) (model: RaceStatusUpdateInput) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(streamId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _ ->
            Error
                { Detail = "The race has ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

        | None, Some _ ->
            Error
                { Detail = "The race has not started but has ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None ->
            Error
                { Detail = "The race has already started"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            let proposedRaceStartTimeChanged =
                RaceDelayed(model.ProposedRaceStartTime.Value)

            session.Events.Append(streamId, proposedRaceStartTimeChanged)
            |> ignore

            session.SaveChanges()
            Ok()


    let updateRace (store: IDocumentStore) (streamId: Guid) (model: RaceStatusUpdateInput) (path: string) =
        use session = store.OpenSession()

        let result =
            match model.Command.ToLower() with
            | Start -> startRace store streamId path
            | Stop -> stopRace store streamId path
            | Restart -> restartRace store streamId path
            | RedFlag -> redflagRace store streamId path
            | OpenPitLane -> openPitLane store streamId path
            | ClosePitLane -> closePitLane store streamId path
            | DelayStartTime -> delayStartTime store streamId model path
            | _ ->
                Error
                    { Detail = "Race command failed, unknown command"
                      Status = 409
                      Title = "Race command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }

        result

    let recordPitLaneEntry (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            let car =
                race.Cars |> Array.find (fun x -> x.Id = carId)

            match car.Driver.Retired, car.Driver.BlackFlagged with
            | true, true
            | false, true
            | true, false ->
                Error
                    { Detail = "The driver has retired/black flagged"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false, false ->
                match car.InPitLane with
                | false ->
                    let carEnteredPitLane =
                        CarEnteredPitLane(carId, DateTimeOffset.UtcNow)


                    session.Events.Append(raceId, carEnteredPitLane)
                    |> ignore

                    session.SaveChanges()
                    Ok()
                | true ->
                    Error
                        { Detail = "The car is already in the pitlane"
                          Status = 409
                          Title = "Car command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }


    let recordPitLaneExit (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            let car =
                race.Cars |> Array.find (fun x -> x.Id = carId)

            match car.Driver.Retired, car.Driver.BlackFlagged with
            | true, true
            | false, true
            | true, false ->
                Error
                    { Detail = "The driver has retired/black flagged"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false, false ->
                match car.InPitLane with
                | true ->
                    let carExitedPitLane =
                        CarExitedPitLane(carId, DateTimeOffset.UtcNow)


                    session.Events.Append(raceId, carExitedPitLane)
                    |> ignore

                    session.SaveChanges()
                    Ok()
                | false ->
                    Error
                        { Detail = "The car is not in the pitlane"
                          Status = 409
                          Title = "Car command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }

    let recordPitBoxEntry (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            let car =
                race.Cars |> Array.find (fun x -> x.Id = carId)

            match car.Driver.Retired, car.Driver.BlackFlagged with
            | true, true
            | false, true
            | true, false ->
                Error
                    { Detail = "The driver has retired/black flagged"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false, false ->
                match car.InPitLane with
                | true ->
                    match car.InPitBox with
                    | false ->
                        let carEnteredPitBox =
                            CarEnteredPitBox(carId, DateTimeOffset.UtcNow)

                        session.Events.Append(raceId, carEnteredPitBox)
                        |> ignore

                        session.SaveChanges()
                        Ok()
                    | true ->
                        Error
                            { Detail = "The car is already in the pitbox"
                              Status = 409
                              Title = "Car command failed"
                              Instance = path
                              Type = "https://example.net/validation-error" }
                | false ->
                    Error
                        { Detail = "The car is not in the pitlane"
                          Status = 409
                          Title = "Car command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }


    let recordPitBoxExit (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            let car =
                race.Cars |> Array.find (fun x -> x.Id = carId)

            match car.Driver.Retired, car.Driver.BlackFlagged with
            | true, true
            | false, true
            | true, false ->
                Error
                    { Detail = "The driver has retired/black flagged"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false, false ->
                match car.InPitLane with
                | true ->
                    match car.InPitBox with
                    | true ->
                        let carExitedPitBox =
                            CarExitedPitBox(carId, DateTimeOffset.UtcNow)

                        session.Events.Append(raceId, carExitedPitBox)
                        |> ignore

                        session.SaveChanges()
                        Ok()
                    | false ->
                        Error
                            { Detail = "The car is not in the pitbox"
                              Status = 409
                              Title = "Car command failed"
                              Instance = path
                              Type = "https://example.net/validation-error" }
                | false ->
                    Error
                        { Detail = "The car is not in the pitlane"
                          Status = 409
                          Title = "Car command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }

    let recordTyreChanged (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            let car =
                race.Cars |> Array.find (fun x -> x.Id = carId)

            match car.Driver.Retired, car.Driver.BlackFlagged with
            | true, true
            | false, true
            | true, false ->
                Error
                    { Detail = "The driver has retired/black flagged"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false, false ->
                match car.InPitBox with
                | true ->
                    let tyreChanged =
                        TyreChanged(carId, DateTimeOffset.UtcNow)

                    session.Events.Append(raceId, tyreChanged)
                    |> ignore

                    session.SaveChanges()
                    Ok()
                | false ->
                    Error
                        { Detail = "The car is not in the pitbox"
                          Status = 409
                          Title = "Car command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }


    let recordNoseChanged (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            let car =
                race.Cars |> Array.find (fun x -> x.Id = carId)

            match car.Driver.Retired, car.Driver.BlackFlagged with
            | true, true
            | false, true
            | true, false ->
                Error
                    { Detail = "The driver has retired/black flagged"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false, false ->
                match car.InPitBox with
                | true ->
                    let noseChanged =
                        NoseChanged(carId, DateTimeOffset.UtcNow)

                    session.Events.Append(raceId, noseChanged)
                    |> ignore

                    session.SaveChanges()
                    Ok()
                | false ->
                    Error
                        { Detail = "The car is not in the pitbox"
                          Status = 409
                          Title = "Car command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }

    let recordDownforceChanged (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            let car =
                race.Cars |> Array.find (fun x -> x.Id = carId)

            match car.Driver.Retired, car.Driver.BlackFlagged with
            | true, true
            | false, true
            | true, false ->
                Error
                    { Detail = "The driver has retired/black flagged"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false, false ->
                match car.InPitBox with
                | true ->
                    let downforceChanged =
                        DownforceChanged(carId, DateTimeOffset.UtcNow)

                    session.Events.Append(raceId, downforceChanged)
                    |> ignore

                    session.SaveChanges()
                    Ok()
                | false ->
                    Error
                        { Detail = "The car is not in the pitbox"
                          Status = 409
                          Title = "Car command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }

    let recordPenaltyPoints (store: IDocumentStore)
                            (raceId: Guid)
                            (carId: Guid)
                            (model: CarStatusUpdateInput)
                            (path: string)
                            =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, Some _
        | Some _, None _ ->

            let penaltyPointsApplied =
                PenaltyPointsApplied(carId, model.PenaltyPoints.Value)

            session.Events.Append(raceId, penaltyPointsApplied)
            |> ignore

            session.SaveChanges()
            Ok()

    let recordDriveThroughPenalty (store: IDocumentStore)
                                  (raceId: Guid)
                                  (carId: Guid)
                                  (model: CarStatusUpdateInput)
                                  (path: string)
                                  =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }

        | Some _, None _ ->

            let driveThroughPenaltyApplied =
                DriveThroughPenaltyApplied(carId, model.DriveThroughPenalty.Value)

            session.Events.Append(raceId, driveThroughPenaltyApplied)
            |> ignore

            session.SaveChanges()
            Ok()

    let recordDriverRetired (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->

            let driverRetired = DriverRetired(carId)

            session.Events.Append(raceId, driverRetired)
            |> ignore

            session.SaveChanges()
            Ok()

    let recordBlackFlag (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->

            let driverBlackFlagged = DriverBlackFlagged(carId)

            session.Events.Append(raceId, driverBlackFlagged)
            |> ignore

            session.SaveChanges()
            Ok()

    let recordCarCrash (store: IDocumentStore) (raceId: Guid) (carId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Car command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            let car =
                race.Cars |> Array.find (fun x -> x.Id = carId)

            match car.Driver.Retired, car.Driver.BlackFlagged with
            | true, true
            | false, true
            | true, false ->
                Error
                    { Detail = "The driver has retired/black flagged"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false, false ->
                
                    let carCrashed =
                        CarCrashed(carId, DateTimeOffset.UtcNow)

                    session.Events.Append(raceId, carCrashed)
                    |> ignore

                    session.SaveChanges()
                    Ok()
                

    let updateCar (store: IDocumentStore) (raceId: Guid) (carId: Guid) (model: CarStatusUpdateInput) (path: string) =
        use session = store.OpenSession()

        let result =
            match model.Command.ToLower() with
            | EnterPitLane -> recordPitLaneEntry store raceId carId path
            | ExitPitLane -> recordPitLaneExit store raceId carId path
            | EnterPitBox -> recordPitBoxEntry store raceId carId path
            | ExitPitBox -> recordPitBoxExit store raceId carId path
            | ChangeTyre -> recordTyreChanged store raceId carId path
            | ChangeNose -> recordNoseChanged store raceId carId path
            | ChangeDownforce -> recordDownforceChanged store raceId carId path
            | ApplyPenaltyPoints -> recordPenaltyPoints store raceId carId model path
            | ApplyDriveThroughPenalty -> recordDriveThroughPenalty store raceId carId model path
            | RetireDriver -> recordDriverRetired store raceId carId path
            | BlackFlag -> recordBlackFlag store raceId carId path
            | CarCrash -> recordCarCrash store raceId carId path
            | _ ->
                Error
                    { Detail = "Car command failed, unknown command"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }

        result

    let registerCars (store: IDocumentStore) (raceId: Guid) (message: RegisterCarInput) path =
        use session = store.OpenSession()

        let carId = Guid.NewGuid()

        let cars =
            message.Cars
            |> List.map (fun x ->
                { Driver =
                      { DriverId = x.DriverId
                        BlackFlagged = false
                        PenaltyPoints = 0
                        DriveThroughPenaltyInSeconds = 0
                        Retired = false }
                  Team = x.Team

                  TyreChanged = Array.empty<DateTimeOffset>
                  NoseChanged = Array.empty<DateTimeOffset>
                  DownforceChanged = Array.empty<DateTimeOffset>
                  EnteredPitLane = Array.empty<DateTimeOffset>
                  ExitedPitLane = Array.empty<DateTimeOffset>
                  InPitLane = false
                  EnteredPitBox = Array.empty<DateTimeOffset>
                  ExitedPitBox = Array.empty<DateTimeOffset>
                  InPitBox = false
                  Crashes = Array.empty<DateTimeOffset>
                  Id = carId })
            |> Array.ofList

        let carRegistered = CarsRegistered(cars)

        session.Events.Append(raceId, carRegistered)
        |> ignore

        session.SaveChanges()
        Ok carId

    //TODO check for already registered cars
    //TODO check no more than 26 cars registered

    let handleDriverRegistered (store: IDocumentStore) (message: DriverInput) path =
        use session = store.OpenSession()

        try
            let stream = session.Events.StartStream<Driver>()

            let driverRegistered = DriverRegistered(message.Name)

            session.Events.Append(stream.Id, driverRegistered)
            |> ignore

            session.SaveChanges()

            Ok stream.Id
        with :? MartenCommandException ->
            Error
                { Detail = "Please ensure you enter a unique name for a driver"
                  Status = 422
                  Title = "Driver command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }


    let getDrivers (store: IDocumentStore) =
        use session = store.OpenSession()

        session.Query<Driver>().ToArray()

    let getDriverById (store: IDocumentStore) (driverId: Guid) =
        use session = store.OpenSession()

        session
            .Query<Driver>()
            .First(fun x -> x.Id = driverId)

    let getPitStops (store: IDocumentStore) (raceId: Guid) =
        use sessions = store.OpenSession()

        sessions
            .Query<PitstopSummary>()
            .Where(fun x -> x.Id = raceId)
            .ToArray()

    let getPitStopsByCar (store: IDocumentStore) (raceId: Guid) (carId: Guid) =
        use sessions = store.OpenSession()

        sessions
            .Query<PitstopSummary>()
            .Where(fun x -> x.Id = raceId && x.CarId = carId)
            .ToArray()

    let startLap (store: IDocumentStore) (raceId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->

            let lapStarted = LapStarted(DateTimeOffset.UtcNow)

            session.Events.Append(raceId, lapStarted)
            |> ignore

            session.SaveChanges()
            Ok()

    let deploySafetyCar (store: IDocumentStore) (raceId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->

            match race.SafetyCarOnTrack with
            | true ->
                Error
                    { Detail = "The safety car has already been deployed"
                      Status = 409
                      Title = "Lap command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false ->
                match race.VirtualSafetyCarDeployed with
                | true ->
                    Error
                        { Detail =
                              "The virtual safety car has already been deployed. Recall the virtual safety car before deploying the safety car"
                          Status = 409
                          Title = "Lap command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }
                | false ->
                    let safetyCarDeployed =
                        SafetyCarDeployed(DateTimeOffset.UtcNow, race.CurrentLap)

                    session.Events.Append(raceId, safetyCarDeployed)
                    |> ignore

                    session.SaveChanges()
                    Ok()

    let recallSafetyCar (store: IDocumentStore) (raceId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->

            match race.SafetyCarOnTrack with
            | false ->
                Error
                    { Detail = "The safety car is not on the track to be recalled"
                      Status = 409
                      Title = "Lap command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | true ->
                match race.VirtualSafetyCarDeployed with
                | true ->
                    Error
                        { Detail = "You cannot recall the safety car when the virtual safety car has been deployed"
                          Status = 409
                          Title = "Lap command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }
                | false ->
                    let safetyCarRecalled =
                        SafetyCarRecalled(DateTimeOffset.UtcNow, race.CurrentLap)

                    session.Events.Append(raceId, safetyCarRecalled)
                    |> ignore

                    session.SaveChanges()
                    Ok()

    let deployVirtualSafetyCar (store: IDocumentStore) (raceId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->

            match race.VirtualSafetyCarDeployed with
            | true ->
                Error
                    { Detail = "The virtual safety car has already been deployed"
                      Status = 409
                      Title = "Lap command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | false ->
                match race.SafetyCarOnTrack with
                | true ->
                    Error
                        { Detail =
                              "The safety car has already been deployed. Recall the safety car before deploying the virtual safety car"
                          Status = 409
                          Title = "Lap command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }
                | false ->
                    let virtualSafetyCarDeployed =
                        VirtualSafetyCarDeployed(DateTimeOffset.UtcNow, race.CurrentLap)

                    session.Events.Append(raceId, virtualSafetyCarDeployed)
                    |> ignore

                    session.SaveChanges()
                    Ok()


    let recallVirtualSafetyCar (store: IDocumentStore) (raceId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<Race>(raceId)

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            Error
                { Detail = "The race hasn't started"
                  Status = 409
                  Title = "Lap command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->

            match race.VirtualSafetyCarDeployed with
            | false ->
                Error
                    { Detail = "The virtual safety car has not been deployed and can't be recalled"
                      Status = 409
                      Title = "Lap command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }
            | true ->
                match race.SafetyCarOnTrack with
                | true ->
                    Error
                        { Detail = "You cannot recall the virtual safety car when the safety car has been deployed"
                          Status = 409
                          Title = "Lap command failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }
                | false ->
                    let virtualSafetyCarRecalled =
                        VirtualSafetyCarRecalled(DateTimeOffset.UtcNow, race.CurrentLap)

                    session.Events.Append(raceId, virtualSafetyCarRecalled)
                    |> ignore

                    session.SaveChanges()
                    Ok()


    let updateLap (store: IDocumentStore) (raceId: Guid) (model: LapUpdateInput) (path: string) =
        use session = store.OpenSession()

        let result =
            match model.Command.ToLower() with
            | StartLap -> startLap store raceId path
            | DeploySafetyCar -> deploySafetyCar store raceId path
            | RecallSafetyCar -> recallSafetyCar store raceId path
            | DeployVirtualSafetyCar -> deployVirtualSafetyCar store raceId path
            | RecallVirtualSafetyCar -> recallVirtualSafetyCar store raceId path
            | _ ->
                Error
                    { Detail = "Lap command failed, unknown command"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }

        result
