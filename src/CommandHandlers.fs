namespace F1ES

module CommandHandlers =

    open System
    open F1ES.Events
    open F1ES.OutputModel
    open Marten
    open F1ES.InputModel

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

    let updateRaceStatus (store: IDocumentStore) (streamId: Guid) (status: string) =
        use session = store.OpenSession()

        match status.ToLower() with
        | "start" ->
            let raceStarted = RaceStarted(DateTimeOffset.UtcNow)

            session.Events.Append(streamId, raceStarted)
            |> ignore
        | "stop" ->
            let raceEnded = RaceEnded(DateTimeOffset.UtcNow)

            session.Events.Append(streamId, raceEnded)
            |> ignore
        | _ -> failwith "Something went wrong"

        session.SaveChanges()
