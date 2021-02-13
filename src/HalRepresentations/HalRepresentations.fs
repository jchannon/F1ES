namespace F1ES

open F1ES.HTTPHandlers
open F1ES.Projections
open F1ES.Constants
open Hallo

type RaceSummaryRepresentation() =
    inherit Hal<RaceSummary>()

    interface IHalLinks<RaceSummary> with
        member this.LinksFor(race) =
            let raceIdUrl =
                (sprintf "/race/%O" (race.Id.ToString()))

            let links = [ Link(Link.Self, raceIdUrl) ]

            let startlinks =
                match race.RaceStarted, race.RaceEnded with
                | None, None -> [ Link(Start, raceIdUrl) ]
                | _ -> []

            let stoplinks =
                match race.RaceStarted, race.RaceEnded with
                | Some _, None -> [ Link(Stop, raceIdUrl) ]
                | _ -> []

            let restartinks =
                match race.RaceStarted, race.RaceEnded with
                | Some _, None -> [ Link(Restart, raceIdUrl) ]
                | _ -> []

            let redflaginks =
                match race.RedFlaggedTime, race.RaceStarted, race.RaceEnded with
                | None, None, None
                | None, Some _, None -> [ Link(RedFlag, raceIdUrl) ]
                | _ -> []

            let openpitlanelinks =
                match race.PitLaneOpen with
                | false -> [ Link(OpenPitLane, raceIdUrl) ]
                | _ -> []

            let closepitlanelinks =
                match race.PitLaneOpen with
                | true -> [ Link(ClosePitLane, raceIdUrl) ]
                | _ -> []

            let delayracelinks =
                match race.RaceStarted, race.RaceEnded with
                | None, None -> [ Link(DelayStartTime, raceIdUrl) ]
                | _ -> []

            let alllinks =
                links
                @ startlinks
                  @ stoplinks
                    @ restartinks
                      @ redflaginks
                        @ openpitlanelinks
                          @ closepitlanelinks @ delayracelinks

            alllinks |> Seq.ofList

[<AbstractClass>]
type ArrayRepresentation<'T>(id: string, itemLinks: IHalLinks<'T>) =
    inherit Hal<'T array>()

    interface IHalState<'T array> with
        member this.StateFor(resource) = {| Count = resource.Length |} :> obj

    interface IHalLinks<RaceSummary> with
        member this.LinksFor(resource) =
            let raceIdUrl = (sprintf "/race/%O" id)

            let links = [ Link(Link.Self, raceIdUrl) ]

            links |> Seq.ofList

type RaceSummaryCarRepresentation() =
    inherit Hal<HalCars>()

    interface IHalState<HalCars> with
        member this.StateFor(resource) =
            {| Count = resource.Cars.Length |} :> obj

    interface IHalEmbedded<HalCars> with
        member this.EmbeddedFor(resource) = {| Cars = resource.Cars |} :> obj

    interface IHalLinks<HalCars> with
        member this.LinksFor(resource) =

            let links =
                [ Link(Link.Self, resource.ResourceOwner) ]

            links |> Seq.ofList
