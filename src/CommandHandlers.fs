namespace F1ES

module CommandHandlers =

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