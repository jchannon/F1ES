namespace F1ES

open System
open System.Linq.Expressions
open System.Text.Json
open System.Text.Json.Serialization
open F1ES.Aggregates
open Giraffe.Serialization
open Marten.Schema
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Giraffe
open Marten
open F1ES.HTTPHandlers
open F1ES.Projections

type FunAs() =
    static member MyExpression<'T, 'TResult>(e: Expression<Func<'T, 'TResult>>) = e


type Startup(configuration: IConfiguration) =
    let appConfiguration = AppConfiguration()

    do configuration.Bind(appConfiguration)

    let webApp =
        choose [ route "/"
                 >=> GET
                 >=> text "There's no place like 127.0.0.1"
                 route "/race" >=> POST >=> scheduleRaceHandler
                 GET >=> routef "/race/%O" getRaceHandler
                 POST >=> routef "/race/%O" updateRace ]

    member __.ConfigureServices(services: IServiceCollection) =

        let options = JsonSerializerOptions()
        options.Converters.Add(JsonFSharpConverter())
        options.Converters.Add(JsonStringEnumConverter())
        options.PropertyNameCaseInsensitive <- true
        //options.IgnoreNullValues <- true

        services.AddSingleton<IJsonSerializer>(SystemTextJsonSerializer(options))
        |> ignore

        let titleIndex =
            FunAs.MyExpression(fun (x: RaceAggregate) -> x.Title :> obj)

        services.AddMarten(fun x ->
            x.Connection(appConfiguration.ConnectionString)

            x.Events.InlineProjections.AggregateStreamsWith<RaceAggregate>()
            |> ignore

            x.Events.InlineProjections.Add<RaceProjection>()

            x
                .Schema
                .For<RaceAggregate>()
                .UniqueIndex(UniqueIndexType.Computed, titleIndex)
            |> ignore)

        |> ignore

        services.AddGiraffe() |> ignore

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =

        app.UseGiraffe webApp
