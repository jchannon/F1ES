namespace F1ES

open System
open F1ES.Events
open F1ES.Model
open Giraffe
open Marten
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.DependencyInjection

module Handlers =
    let initializeRaceHandler: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                //TODO Put this in a command handler
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                use session = store.OpenSession()
                let stream = session.Events.StartStream<Race>()

                let raceinitialised =
                    RaceInitialised
                        ("Brazil",
                         "Interlagos",
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
