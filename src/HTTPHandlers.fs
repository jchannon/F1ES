namespace F1ES

module HTTPHandlers =
    open ModelBinding
    open System
    open F1ES.OutputModel
    open Giraffe
    open Marten
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.Extensions.DependencyInjection
    open F1ES.InputModel

    let initializeRaceHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {

                let! model = tryBindJson<RaceInitialisedInput> (ctx)

                match model with
                | Ok x ->

                    let store =
                        ctx.RequestServices.GetRequiredService<IDocumentStore>()

                    let streamId =
                        CommandHandlers.handleRaceInitialised store x

                    ctx.SetStatusCode 201

                    ctx.SetHttpHeader "Location" (sprintf "http://localhost:5000/race/%O" streamId)
                    //TODO Set cache headers

                    return! next ctx

                | Error errorHandler -> return! errorHandler next ctx
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
