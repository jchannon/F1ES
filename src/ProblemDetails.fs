namespace F1ES

module ProblemDetails =

    open Microsoft.AspNetCore.Http
    open Giraffe
    open FSharp.Control.Tasks

    type ProblemDetails =
        { Type: string
          Title: string
          Instance: string
          Detail: string
          Status: int }

    let problemDetailsHandler model: HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                ctx.SetContentType "application/problem+json"
                let serializer = ctx.GetJsonSerializer()

                return!
                    serializer.SerializeToBytes model
                    |> ctx.WriteBytesAsync
            }