open System
open System.Text.Json
open System.Text.Json
open System.Text.Json.Serialization
open F1ES
open F1ES.Events
open F1ES.Model
open Marten
open Newtonsoft.Json
open Newtonsoft.Json
open Newtonsoft.Json
open Newtonsoft.Json

[<EntryPoint>]
let main argv =

    let store =
        DocumentStore.For("host=localhost;database=marten;password=password1;username=postgres")

    use session = store.OpenSession()

    let stream = session.Events.StartStream<Race>()

    //We could create our own Id and not call StartStream and just call Append but the Id we pass in would need to be
    //a CombGuid compatible with the database
    let streamId = stream.Id


    let raceinitialised =
        RaceInitialised
            ("Brazil",
             "Interlagos",
             [| { Team = Mercedes
                  DriverName = "Lewis Hamilton" }
                { Team = Mercedes
                  DriverName = "Valtteri Bottas" } |])

    let raceStarted = RaceStarted(DateTimeOffset.UtcNow)

    let raceEnded = RaceEnded(DateTimeOffset.UtcNow)

    session.Events.Append(streamId, raceinitialised, raceStarted, raceEnded)
    |> ignore

    session.SaveChanges()

    let returnedRace =
        session.Events.AggregateStream<Race>(streamId)

    let options = JsonSerializerOptions()
    options.WriteIndented <- true
    options.Converters.Add(JsonFSharpConverter())
    printfn "%A" (System.Text.Json.JsonSerializer.Serialize(returnedRace, options))

    0 // return an integer exit code
