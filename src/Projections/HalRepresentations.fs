namespace F1ES

open F1ES.Projections
open F1ES.Constants

type RaceSummaryRepresentation() =
    inherit Hallo.Hal<RaceSummary>()

    interface Hallo.IHalLinks<RaceSummary> with
        member this.LinksFor(race) =
            let raceIdUrl =
                (sprintf "/race/%O" (race.Id.ToString()))

            let links =
                [ Hallo.Link(Hallo.Link.Self, raceIdUrl) ]

            let startlinks =
                match race.RaceStarted, race.RaceEnded with
                | None, None -> [ Hallo.Link(Start, raceIdUrl) ]
                | _ -> []

            let stoplinks =
                match race.RaceStarted, race.RaceEnded with
                | Some _, None -> [ Hallo.Link(Stop, raceIdUrl) ]
                | _ -> []

            let restartinks =
                match race.RaceStarted, race.RaceEnded with
                | Some _, None -> [ Hallo.Link(Restart, raceIdUrl) ]
                | _ -> []

            let redflaginks =
                match race.RedFlaggedTime, race.RaceStarted, race.RaceEnded with
                | None, None, None
                | None, Some _, None -> [ Hallo.Link(RedFlag, raceIdUrl) ]
                | _ -> []

            let openpitlanelinks =
                match race.PitLaneOpen with
                | false -> [ Hallo.Link(OpenPitLane, raceIdUrl) ]
                | _ -> []

            let closepitlanelinks =
                match race.PitLaneOpen with
                | true -> [ Hallo.Link(ClosePitLane, raceIdUrl) ]
                | _ -> []

            let delayracelinks =
                match race.RaceStarted, race.RaceEnded with
                | None, None -> [ Hallo.Link(DelayStartTime, raceIdUrl) ]
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
