namespace F1ES

module InputModel =
    open System
    open F1ES.ModelBinding
    open Giraffe
    open ProblemDetails
       
    [<CLIMutable>]
    type CarInput = {
        Team:Team
        Driver:string
    }

    [<CLIMutable>]
    type RaceScheduledInput =
        { Country: string
          TrackName: string
          ScheduledStartTime: DateTimeOffset
          Cars: CarInput list
          Title: string }
        member this.HasErrors() =

            let trackValidation =
                if String.IsNullOrWhiteSpace this.TrackName
                then Some "Track is required"
                else None

            let countryValidation =
                if String.IsNullOrWhiteSpace this.Country then Some "Country is required" else None

            let scheduledStartTimeValidation =
                if this.ScheduledStartTime = DateTimeOffset.MinValue
                then Some "ProposedRaceStartTime is required"
                else None

            let carsValidation =
                match this.Cars with
                | [] -> Some "Cars is required"
                | _ -> None
                
            let titleValidation =
                if String.IsNullOrWhiteSpace this.Title then Some "Title is required" else None

            [| trackValidation
               countryValidation
               scheduledStartTimeValidation
               carsValidation
               titleValidation |]
            |> Array.choose id

        //None

        interface IProblemDetailsValidation<RaceScheduledInput> with
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

    [<Literal>]
    let Restart = "restart"

    [<Literal>]
    let RedFlag = "redflag"

    [<Literal>]
    let OpenPitLane = "openpitlane"

    [<Literal>]
    let ClosePitLane = "closepitlane"

    [<Literal>]
    let ChangeProposedStartTime = "changestarttime"

    let Commands =
        [| Start
           Stop
           Restart
           RedFlag
           OpenPitLane
           ClosePitLane
           ChangeProposedStartTime |]

    [<CLIMutable>]
    type RaceStatusUpdateInput =
        { Command: string
          ProposedRaceStartTime: DateTimeOffset option }
        member this.HasErrors() =
            let statusValidation =
                match Commands
                      |> Array.exists (fun x -> x.Equals(this.Command, StringComparison.OrdinalIgnoreCase)) with
                | true -> None
                | false ->
                    let commands = String.Join(" or ", Commands)
                    Some(sprintf "Status needs to be %s" commands)

            let proposedRaceStartTimeValidation =
                match this.Command with
                | ChangeProposedStartTime ->
                    match this.ProposedRaceStartTime with
                    | Some _ -> None
                    | None -> Some "A proposed start time is required"
                | _ -> None

            [| statusValidation
               proposedRaceStartTimeValidation |]
            |> Array.choose id

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

                    Error(RequestErrors.unprocessableEntity (problemDetailsHandler problemDetails)) 
