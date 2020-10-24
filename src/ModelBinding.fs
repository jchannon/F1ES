namespace F1ES

module ModelBinding =
    open Giraffe
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.AspNetCore.Http

    let tryBindJson<'T when 'T :> IModelValidation<'T>> (ctx: HttpContext) =
        task {
            try

                let! model = ctx.BindJsonAsync<'T>()
                return model.Validate()

            with ex -> return Error(RequestErrors.unprocessableEntity (text (sprintf "Error binding incoming data: %s" ex.Message)))
        }