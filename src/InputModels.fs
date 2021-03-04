namespace F1ES

module InputModels =
    open System
    open F1ES.ModelBinding
    open Giraffe
    open ProblemDetails
    open F1ES.Constants

    [<CLIMutable>]
    type CarInput = { Team: Team; DriverId: Guid }

    [<CLIMutable>]
    type RaceScheduledInput =
        { Country: string
          TrackName: string
          ScheduledStartTime: DateTimeOffset
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

            let titleValidation =
                if String.IsNullOrWhiteSpace this.Title then Some "Title is required" else None

            [| trackValidation
               countryValidation
               scheduledStartTimeValidation
               titleValidation |]
            |> Array.choose id

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

                    Error(RequestErrors.unprocessableEntity (problemDetailsHandler problemDetails))

    let Commands =
        [| Start
           Stop
           Restart
           RedFlag
           OpenPitLane
           ClosePitLane
           DelayStartTime |]

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
                | DelayStartTime ->
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

    let UpdateCarCommands = [| EnterPitLane; ExitPitLane;EnterPitBox;ExitPitBox |]

    [<CLIMutable>]
    type CarStatusUpdateInput =
        { Command: string }
        member this.HasErrors() =
            let statusValidation =
                match UpdateCarCommands
                      |> Array.exists (fun x -> x.Equals(this.Command, StringComparison.OrdinalIgnoreCase)) with
                | true -> None
                | false ->
                    let commands = String.Join(" or ", UpdateCarCommands)
                    Some(sprintf "Status needs to be %s" commands)

            [| statusValidation |] |> Array.choose id

        interface IProblemDetailsValidation<CarStatusUpdateInput> with
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

    [<CLIMutable>]
    type RegisterCarInput =
        { Cars: CarInput list }
        member this.HasErrors() =

            let carsValidation =
                match this.Cars with
                | [] -> Some "Cars is required"
                | _ -> None

            [| carsValidation |] |> Array.choose id

        interface IProblemDetailsValidation<RegisterCarInput> with
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

    [<CLIMutable>]
    type DriverInput =
        { Name: string }
        member this.HasErrors() =

            let nameValidation =
                match String.IsNullOrWhiteSpace(this.Name) with
                | true -> Some "Driver name is required"
                | _ -> None

            [| nameValidation |] |> Array.choose id

        interface IProblemDetailsValidation<DriverInput> with
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

    let UpdateLapCommands = [| StartLap; DeploySafetyCar; RecallSafetyCar; DeployVirtualSafetyCar; RecallVirtualSafetyCar |]

    [<CLIMutable>]
    type LapUpdateInput =
        { Command: string }
        member this.HasErrors() =
            let statusValidation =
                match UpdateLapCommands
                      |> Array.exists (fun x -> x.Equals(this.Command, StringComparison.OrdinalIgnoreCase)) with
                | true -> None
                | false ->
                    let commands = String.Join(" or ", UpdateLapCommands)
                    Some(sprintf "Status needs to be %s" commands)

            [| statusValidation |] |> Array.choose id

        interface IProblemDetailsValidation<LapUpdateInput> with
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