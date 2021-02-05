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

                let! model = tryBindJsonBody<RaceInitialisedInput> (ctx)

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
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                let returnedRace = CommandHandlers.getRace store streamId
                //todo return links with link rels on how to update a race
                //todo don't return a start link when the race has started
                return! ctx.WriteJsonAsync returnedRace
            }
            
    let updateRace (streamId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()
                    
                let! model = tryBindJsonBody<RaceStatusUpdateInput> (ctx)

                match model with
                | Ok x ->
                    do CommandHandlers.updateRace store streamId x.Command
                    
                    ctx.SetStatusCode 204
                    return! next ctx
                    
                | Error errorHandler -> return! errorHandler next ctx

                
            }
