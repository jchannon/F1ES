namespace F1ES

open System
open F1ES.Events
open F1ES.Model
open Giraffe
open Marten
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.DependencyInjection



[<CLIMutable>]
type RaceInitialisedInput =
    { Country: string
      TrackName: string }
    member this.HasErrors() =
        if this.Country <> "Brazil" then Some "Country is required"
        else if this.TrackName.Length = 0 then Some "TrackName is required"
        else None

    interface IModelValidation<RaceInitialisedInput> with
        member this.Validate() =
            match this.HasErrors() with
            | Some msg -> Error(RequestErrors.unprocessableEntity (text msg)) //TODO Problem Details response
            | None -> Ok this

module CommandHandlers =
    let handleRaceInitialised message = 0

module Handlers =
    open ModelBinding

    let initializeRaceHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {

                let! model = tryBindJson<RaceInitialisedInput> (ctx)

                match model with
                | Ok x ->

                    //TODO Put this in a command handler
                    let store =
                        ctx.RequestServices.GetRequiredService<IDocumentStore>()

                    use session = store.OpenSession()

                    let stream = session.Events.StartStream<Race>()

                    let raceinitialised =
                        RaceInitialised
                            (x.Country,
                             x.TrackName,
                             [| { Team = Mercedes
                                  DriverName = "Lewis Hamilton" }
                                { Team = Mercedes
                                  DriverName = "Valtteri Bottas" } |])

                    session.Events.Append(stream.Id, raceinitialised)
                    |> ignore

                    session.SaveChanges()

                    ctx.SetStatusCode 201

                    ctx.SetHttpHeader "Location" (sprintf "http://localhost:5000/race/%O" stream.Id)
                    //TODO Set cache headers


                    return! next ctx

                | Error e -> return! e next ctx
            }

    let getRaceHandler (streamId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                //TODO Put this in a query handler
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                use session = store.OpenSession()

                let returnedRace =
                    session.Events.AggregateStream<Race>(streamId)

                return! ctx.WriteJsonAsync returnedRace
            }
