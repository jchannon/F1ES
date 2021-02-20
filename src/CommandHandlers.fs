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

        match race.RaceStarted, race.RaceEnded with
        | Some _, Some _
        | None, Some _ ->
            Error
                { Detail = "The race has already ended"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | Some _, None _ ->
            //TODO Create proper URI for problem details
            Error
                { Detail = "The race has already started"
                  Status = 409
                  Title = "Race command failed"
                  Instance = path
                  Type = "https://example.net/validation-error" }
        | None, None ->
            let raceStarted = RaceStarted(DateTimeOffset.UtcNow)
            let pitLaneOpened = PitLaneOpened(DateTimeOffset.UtcNow)


            session.Events.Append(streamId, raceStarted, pitLaneOpened)
            |> ignore

            session.SaveChanges()
            Ok()

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

    let updateCar (store: IDocumentStore) (raceId: Guid) (carId: Guid) (model: CarStatusUpdateInput) (path: string) =
        use session = store.OpenSession()

        let result =
            match model.Command.ToLower() with
            | EnterPitLane -> recordPitLaneEntry store raceId carId path
            | ExitPitLane -> recordPitLaneExit store raceId carId path
            | EnterPitBox -> recordPitBoxEntry store raceId carId path
            | ExitPitBox -> recordPitBoxExit store raceId carId path
            | _ ->
                Error
                    { Detail = "Car command failed, unknown command"
                      Status = 409
                      Title = "Car command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }

        result

    let registerCar (store: IDocumentStore) (raceId: Guid) (message: RegisterCarInput) path =
        use session = store.OpenSession()

        let carId = Guid.NewGuid()

        let cars =
            message.Cars
            |> List.map (fun x ->
                { Driver =
                      { DriverId = x.DriverId
                        BlackFlagged = false
                        PenaltyApplied = false
                        PenaltyPointsApplied = 0
                        Retired = false
                        Crashed = false }
                  Team = x.Team

                  TyreChanged = Array.empty<DateTimeOffset option>
                  NoseChanged = Array.empty<DateTimeOffset option>
                  DownforceChanged = Array.empty<DateTimeOffset option>
                  EnteredPitLane = Array.empty<DateTimeOffset>
                  ExitedPitLane = Array.empty<DateTimeOffset>
                  InPitLane = false
                  EnteredPitBox = Array.empty<DateTimeOffset>
                  ExitedPitBox = Array.empty<DateTimeOffset>
                  InPitBox = false
                  Id = carId })
            |> Array.ofList

        let carRegistered = CarRegistered(cars)

        session.Events.Append(raceId, carRegistered)
        |> ignore

        session.SaveChanges()
        Ok carId

    //TODO check for already registered cars

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
