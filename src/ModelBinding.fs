namespace F1ES

module ModelBinding =
    open System
    open System.Text.Json
    open Giraffe
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.AspNetCore.Http
    open F1ES.ProblemDetails

    type IProblemDetailsValidation<'T> =
        abstract Validate: string -> Result<'T, HttpHandler>

    let tryBindJsonBody<'T when 'T :> IProblemDetailsValidation<'T>> (ctx: HttpContext) =
        task {
            try

                let! model = ctx.BindJsonAsync<'T>()
                return model.Validate(ctx.Request.Path.ToString())

            //catch json exception parse missing field and look for a : to get property name out
            with
            | :? JsonException as jsonexception when jsonexception.Message.Contains
                                                         ("missing field", StringComparison.OrdinalIgnoreCase) ->
                let missingField =
                    jsonexception.Message.Substring(jsonexception.Message.IndexOf(":") + 2)

                let errorMessage =
                    sprintf "Please specify a value for the %s property" missingField

                let problemDetails =
                    { Detail = errorMessage
                      Status = 422
                      Title = "Model validation failed"
                      Instance = ctx.Request.Path.ToString()
                      Type = "https://example.net/validation-error" }

                ctx.SetStatusCode 422

                return Error(problemDetailsHandler problemDetails)
            | ex ->
                let problemDetails =
                    { Detail = ex.Message
                      Status = 422
                      Title = "Model validation failed"
                      Instance = ctx.Request.Path.ToString()
                      Type = "https://example.net/validation-error" }

                return Error(problemDetailsHandler problemDetails)
        }

    let tryBindJson<'T when 'T :> IModelValidation<'T>> (ctx: HttpContext) =
        task {
            try

                let! model = ctx.BindJsonAsync<'T>()
                return model.Validate()

            with ex ->
                return
                    Error
                        (RequestErrors.unprocessableEntity (text (sprintf "Error binding incoming data: %s" ex.Message)))
        }
