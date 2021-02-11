namespace F1ES

module CommandHandlers =

    open System
    open F1ES.Events
    open Marten
    open F1ES.InputModel
    open F1ES.ProblemDetails
    open F1ES.Aggregates
    open F1ES.Projections
    open System.Linq

    let handleRaceInitialised (store: IDocumentStore) message =

        use session = store.OpenSession()

        let stream =
            session.Events.StartStream<RaceAggregate>()

        let raceinitialised =
            RaceScheduled
                (message.Country,
                 message.TrackName,
                 [| { Team = Mercedes
                      DriverName = "Lewis Hamilton" }
                    { Team = Mercedes
                      DriverName = "Valtteri Bottas" } |])

        session.Events.Append(stream.Id, raceinitialised)
        |> ignore

        session.SaveChanges()

        stream.Id

    let getRace (store: IDocumentStore) (streamId: Guid) =
        use session = store.OpenSession()

        session
            .Query<Race>()
            .Where(fun x -> x.Id = streamId)
            .Single()

    let startRace (store: IDocumentStore) (streamId: Guid) (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<RaceAggregate>(streamId)

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
            session.Events.AggregateStream<RaceAggregate>(streamId)

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
            session.Events.AggregateStream<RaceAggregate>(streamId)

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
            session.Events.AggregateStream<RaceAggregate>(streamId)

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
            session.Events.AggregateStream<RaceAggregate>(streamId)

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
            session.Events.AggregateStream<RaceAggregate>(streamId)

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

    let changeProposedStartTime (store: IDocumentStore) (streamId: Guid) model (path: string) =
        use session = store.OpenSession()

        let race =
            session.Events.AggregateStream<RaceAggregate>(streamId)

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
            | ChangeProposedStartTime -> changeProposedStartTime store streamId model path
            | _ ->
                Error
                    { Detail = "Race command failed, unknown command"
                      Status = 409
                      Title = "Race command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }

        result
