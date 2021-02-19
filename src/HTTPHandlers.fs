namespace F1ES

module HTTPHandlers =
    open F1ES.Projections
    open ModelBinding
    open System
    open Giraffe
    open Marten
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.Extensions.DependencyInjection
    open F1ES.InputModels
    open F1ES.ProblemDetails
    open F1ES.Hal
    open Microsoft.AspNetCore.Http.Extensions

    let optionsHandler: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                ctx.SetHttpHeader "Allow" "POST, OPTIONS,"
                return! next ctx
            }

    let scheduleRaceHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {

                let! model = tryBindJsonBody<RaceScheduledInput> (ctx)

                match model with
                | Ok x ->

                    let store =
                        ctx.RequestServices.GetRequiredService<IDocumentStore>()

                    let result =
                        CommandHandlers.handleRaceScheduled store x (ctx.Request.Path.ToString())

                    match result with
                    | Ok streamId ->
                        ctx.SetStatusCode 201

                        ctx.SetHttpHeader "Location" (sprintf "%s/%O" (ctx.Request.GetEncodedUrl()) streamId)
                        //TODO Set cache headers

                        return! next ctx
                    | Error e ->
                        ctx.SetStatusCode e.Status
                        return! (problemDetailsHandler e) next ctx

                | Error errorHandler -> return! errorHandler next ctx
            }

    let getRaceHandler (streamId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                let returnedRace = CommandHandlers.getRace store streamId

                return! halHandler returnedRace next ctx
            }

    let updateRace (streamId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                let! model = tryBindJsonBody<RaceStatusUpdateInput> (ctx)

                match model with
                | Ok x ->
                    let updateRaceResult =
                        CommandHandlers.updateRace store streamId x (ctx.Request.Path.ToString())

                    match updateRaceResult with
                    | Ok _ ->
                        ctx.SetStatusCode 204
                        return! next ctx
                    | Error e ->
                        ctx.SetStatusCode e.Status
                        return! problemDetailsHandler e next ctx

                | Error errorHandler -> return! errorHandler next ctx
            }

    let optionsRaceHandler (streamId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                ctx.SetHttpHeader "Allow" "GET, POST, OPTIONS, HEAD"
                return! next ctx
            }

    type HalCars =
        { ResourceOwner: string
          Cars: Car array }

    let getCarsHandler (streamId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                let returnedRace = CommandHandlers.getRace store streamId

                let halCars =
                    { ResourceOwner = (sprintf "/race/%O/cars" streamId)
                      Cars = returnedRace.Cars }

                return! halHandler halCars next ctx
            }

    let registerCarHandler (streamId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! model = tryBindJsonBody<RegisterCarInput> (ctx)

                match model with
                | Ok x ->
                    let store =
                        ctx.RequestServices.GetRequiredService<IDocumentStore>()

                    let result =
                        CommandHandlers.registerCar store streamId x (ctx.Request.Path.ToString())

                    match result with
                    | Ok carId ->
                        ctx.SetStatusCode 201

                        ctx.SetHttpHeader "Location" (sprintf "%s/%O" (ctx.Request.GetEncodedUrl()) carId)
                        //TODO Set cache headers

                        return! next ctx
                    | Error e ->
                        ctx.SetStatusCode e.Status
                        return! (problemDetailsHandler e) next ctx

                | Error errorHandler -> return! errorHandler next ctx
            }

    let optionsRaceCarHandler (streamId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                ctx.SetHttpHeader "Allow" "GET, POST, OPTIONS, HEAD"
                return! next ctx
            }

    type HalCar = { ResourceOwner: string; Car: Car }

    let getCarHandler (streamId: Guid, carId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                let returnedRace = CommandHandlers.getRace store streamId

                let halCars =
                    { ResourceOwner = (sprintf "/race/%O/cars/%O" streamId carId)
                      Car =
                          returnedRace.Cars
                          |> Array.find (fun x -> x.Id = carId) }

                return! halHandler halCars next ctx
            }

    let optionsgetCarHandler (race: Guid, carId: Guid): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                ctx.SetHttpHeader "Allow" "GET, OPTIONS, HEAD"
                return! next ctx
            }

    let registerDriverHandler: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! model = tryBindJsonBody<DriverInput> (ctx)

                match model with
                | Ok x ->

                    let store =
                        ctx.RequestServices.GetRequiredService<IDocumentStore>()

                    let result =
                        CommandHandlers.handleDriverRegistered store x (ctx.Request.Path.ToString())

                    match result with
                    | Ok streamId ->
                        ctx.SetStatusCode 201

                        ctx.SetHttpHeader "Location" (sprintf "%s/%O" (ctx.Request.GetEncodedUrl()) streamId)
                        //TODO Set cache headers

                        return! next ctx
                    | Error e ->
                        ctx.SetStatusCode e.Status
                        return! (problemDetailsHandler e) next ctx

                | Error errorHandler -> return! errorHandler next ctx
            }

    let getDriversHandler: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                let drivers = CommandHandlers.getDrivers store
                return! json drivers next ctx
            }
            
    let optionsDriversHandler:HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                ctx.SetHttpHeader "Allow" "GET, OPTIONS, HEAD, POST"
                return! next ctx
            }
            
    let getDriverHandler (driverId:Guid) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let store =
                    ctx.RequestServices.GetRequiredService<IDocumentStore>()

                let driver = CommandHandlers.getDriverById store driverId
                return! json driver next ctx
            }
            
    let optionsDriverHandler (_:Guid):HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                ctx.SetHttpHeader "Allow" "GET, OPTIONS, HEAD"
                return! next ctx
            }
