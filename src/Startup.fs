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
                 >=> text "There's no place like 127.0.0.1" //TODO Home Document application/json-home?
                 
                 route "/race" >=> POST >=> scheduleRaceHandler
                 route "/race" >=> OPTIONS >=> optionsHandler
                 
                 GET_HEAD >=> routef "/race/%O" getRaceHandler
                 POST >=> routef "/race/%O" updateRace
                 OPTIONS >=> routef "/race/%O" optionsRaceHandler
                 
                 GET_HEAD >=> routef "/race/%O/cars" getCarsHandler
                 POST >=> routef "/race/%O/cars" registerCarHandler
                 GET_HEAD >=> routef "/race/%O/cars/%O" getCarHandler
                 
                 
                 
                 
                 ]

    member __.ConfigureServices(services: IServiceCollection) =

        let options = JsonSerializerOptions()
        options.Converters.Add(JsonFSharpConverter(JsonUnionEncoding.FSharpLuLike))
        options.PropertyNameCaseInsensitive <- true
        //options.IgnoreNullValues <- true

        services.AddSingleton<IJsonSerializer>(SystemTextJsonSerializer(options))
        |> ignore

        let titleIndex =
            FunAs.MyExpression(fun (x: Race) -> x.Title :> obj)


        services.AddMarten(fun x ->
            x.Connection(appConfiguration.ConnectionString)

            x.Events.InlineProjections.AggregateStreamsWith<Race>()
            |> ignore

            x.Events.InlineProjections.Add<RaceProjection>()

            x
                .Schema
                .For<Race>()
                .UniqueIndex(UniqueIndexType.Computed, titleIndex)
            |> ignore)

        |> ignore
        
        let serializerOptions = JsonSerializerOptions()
        serializerOptions.Converters.Add(Hallo.Serialization.HalRepresentationConverter())
        serializerOptions.Converters.Add(Hallo.Serialization.LinksConverter())
        serializerOptions.Converters.Add(JsonFSharpConverter(JsonUnionEncoding.FSharpLuLike))

        services.AddSingleton(serializerOptions) |> ignore
        
        services.AddTransient<RaceSummaryRepresentation>()
        |> ignore
        
//        services.AddTransient<RaceSummaryCarRepresentation>()
//        |> ignore

        services.AddTransient<Hallo.Hal<RaceSummary>, RaceSummaryRepresentation>()
        |> ignore
        
        services.AddTransient<Hallo.Hal<HalCars>, RaceSummaryCarsRepresentation>()
        |> ignore
        
        services.AddTransient<Hallo.Hal<HalCar>, RaceSummaryCarRepresentation>()
        |> ignore

        services.AddGiraffe() |> ignore

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =

        app.UseGiraffe webApp
