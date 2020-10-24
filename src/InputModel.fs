namespace F1ES

module InputModel =
    open Giraffe

    [<CLIMutable>]
    type RaceInitialisedInput =
        { Country: string
          TrackName: string }
        member this.HasErrors() =
//            if this.Country <> "Brazil" then Some "Country is required"
//            else if this.TrackName.Length = 0 then Some "TrackName is required"
//            else
            None

        interface IModelValidation<RaceInitialisedInput> with
            member this.Validate() =
                match this.HasErrors() with
                | Some msg -> Error(RequestErrors.unprocessableEntity (text msg)) //TODO Problem Details response
                | None -> Ok this
