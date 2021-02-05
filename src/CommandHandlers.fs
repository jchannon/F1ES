namespace F1ES

module CommandHandlers =

    open System
    open F1ES.Events
    open F1ES.OutputModel
    open Marten
    open F1ES.InputModel
    open F1ES.ProblemDetails

    let handleRaceInitialised (store: IDocumentStore) message =

        use session = store.OpenSession()

        let stream = session.Events.StartStream<Race>()

        let raceinitialised =
            RaceInitialised
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
        //TODO Get race from a projection
        session.Events.AggregateStream<Race>(streamId)

    let getRaces (store: IDocumentStore) =
        use session = store.OpenSession()
        //TODO Get list of races from a projection
        ()

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

            session.Events.Append(streamId, raceStarted)
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


    let updateRace (store: IDocumentStore) (streamId: Guid) (command: string) (path: string) =
        use session = store.OpenSession()

        let result =
            match command.ToLower() with
            | Start -> startRace store streamId path
            | Stop -> stopRace store streamId path
            | Restart -> restartRace store streamId path
            | _ ->
                Error
                    { Detail = "Race command failed, unknown command"
                      Status = 409
                      Title = "Race command failed"
                      Instance = path
                      Type = "https://example.net/validation-error" }

        result
