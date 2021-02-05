namespace F1ES

module InputModel =
    open System
    open F1ES.ModelBinding
    open Giraffe
    open ProblemDetails

    [<CLIMutable>]
    type RaceInitialisedInput =
        { Country: string
          TrackName: string }
        member this.HasErrors() =

            let trackValidation =
                if String.IsNullOrWhiteSpace this.TrackName
                then Some "Track is required"
                else None

            let countryValidation =
                if String.IsNullOrWhiteSpace this.Country then Some "Country is required" else None

            [| trackValidation; countryValidation |]
            |> Array.choose id

        //None

        interface IProblemDetailsValidation<RaceInitialisedInput> with
            member this.Validate path =
                match this.HasErrors() with
                | [||] -> Ok this
                | x ->
                    let errors = String.Join(",", x)

                    let problemDetails =
                        { Detail = errors
                          Status = 422
                          Title = "Model validation failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }

                    Error(RequestErrors.unprocessableEntity (problemDetailsHandler problemDetails)) //TODO Problem Details response

    [<Literal>]
    let Start = "start"
    [<Literal>]
    let Stop = "stop"

    [<CLIMutable>]
    type RaceStatusUpdateInput =
        { Command: string }
        member this.HasErrors() =
            //TODO Can't seem to deserialize to discriminated union of Status with value Start of "Start"
            let statusValidation = match this.Command.ToLower() with
                                    |Start | Stop -> None
                                    |_ -> Some "Status needs to be Start or Stop"
           
            [|statusValidation|] |> Array.choose id

        interface IProblemDetailsValidation<RaceStatusUpdateInput> with
            member this.Validate path =
                match this.HasErrors() with
                | [||] -> Ok this
                | x ->
                    let errors = String.Join(",", x)

                    let problemDetails =
                        { Detail = errors
                          Status = 422
                          Title = "Model validation failed"
                          Instance = path
                          Type = "https://example.net/validation-error" }

                    Error(RequestErrors.unprocessableEntity (problemDetailsHandler problemDetails)) //TODO Problem Details response
