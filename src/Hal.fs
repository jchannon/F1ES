namespace F1ES

module Hal =

    open System
    open System.Text.Json
    open FSharp.Control.Tasks
    open Microsoft.AspNetCore.Http
    open Giraffe
    open Microsoft.Extensions.DependencyInjection

    let getRepresentationGenerator (services: IServiceProvider) (resourceType: Type) =
        let representationType =
            typedefof<Hallo.Hal<_>>
                .MakeGenericType(resourceType)

        let representation = services.GetService(representationType)

        match representation with
        | null -> None
        | _ -> Some(unbox<Hallo.IHal> representation)

    let halHandler (halObject: Object) =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let representationGenerator =
                    getRepresentationGenerator ctx.RequestServices (halObject.GetType())

                match representationGenerator with
                | Some x ->
                    let! representation = x.RepresentationOfAsync(halObject)

                    let serializerOptions =
                        ctx.RequestServices.GetRequiredService<JsonSerializerOptions>()

                    let json =
                        JsonSerializer.Serialize(representation, serializerOptions)

                    ctx.SetContentType "application/hal+json"
                    return! json |> ctx.WriteStringAsync


                | None -> return! setStatusCode 204 earlyReturn ctx
            }
