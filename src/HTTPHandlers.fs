namespace F1ES

module HTTPHandlers =
    open ModelBinding
    open System
    open Giraffe
    open Marten
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.Extensions.DependencyInjection
    open F1ES.InputModel
    open F1ES.ProblemDetails

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
                    let updateRaceResult = CommandHandlers.updateRace store streamId x (ctx.Request.Path.ToString())
                    match updateRaceResult with
                    |Ok _ ->
                        ctx.SetStatusCode 204
                        return! next ctx
                    |Error e ->
                        ctx.SetStatusCode e.Status 
                        return! problemDetailsHandler e next ctx
                    
                | Error errorHandler -> return! errorHandler next ctx

                
            }
