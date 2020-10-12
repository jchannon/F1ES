open System
open F1ES
open F1ES.Events
open F1ES.Model
open Marten

[<EntryPoint>]
let main argv =
    
    let store = DocumentStore
                    .For("host=localhost;database=marten;password=password1;username=postgres")
                   
    use session = store.OpenSession()

    let stream = session.Events.StartStream<Race>()
    
    let streamId = stream.Id

    let raceStarted = RaceStarted(Guid.NewGuid(), DateTimeOffset.UtcNow)
    
    session.Events.Append(streamId, raceStarted) |> ignore
    
    let raceEnded = RaceEnded(DateTimeOffset.UtcNow)
    
    session.Events.Append(streamId, raceEnded) |> ignore

    
    session.SaveChanges()
    
    let returnedRace = session.Events.AggregateStream<Race>(streamId)
    
    let raceStartedTime = match returnedRace.RaceStarted with
                            |Some x -> x.ToString("O")
                            |None -> "N/A"
    printfn "%A %A %s" streamId returnedRace.Id raceStartedTime
    
    0 // return an integer exit code
