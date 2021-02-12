namespace F1ES

open F1ES.Projections

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
                | None, None -> [ Hallo.Link("start", raceIdUrl) ]
                | _ -> []

            let alllinks = links @ startlinks

            alllinks |> Seq.ofList
