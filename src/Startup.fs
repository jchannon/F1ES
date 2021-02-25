namespace F1ES

open System
open System.Linq.Expressions
open System.Text.Encodings.Web
open System.Text.Json
open System.Text.Json.Serialization
open F1ES.Aggregates
open Hallo
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
open Microsoft.FSharpLu.Json
open Newtonsoft.Json

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
                 
                 GET_HEAD >=> routef "/race/%O/pitstops" getPitStopsHandler
                 GET_HEAD >=> routef "/race/%O/pitstops/%O" getPitStopsByCarHandler

                 GET_HEAD >=> routef "/race/%O/cars" getCarsHandler
                 POST >=> routef "/race/%O/cars" registerCarHandler
                 OPTIONS >=> routef "/race/%O/cars" optionsRaceCarHandler

                 GET_HEAD >=> routef "/race/%O/cars/%O" getCarHandler
                 POST >=> routef "/race/%O/cars/%O" addCarCommandHandler
                 OPTIONS >=> routef "/race/%O/cars/%O" optionsgetCarHandler
                 
                 POST >=> routef "/race/%O/laps" updateLapHandler

                 route "/drivers" >=> POST >=> registerDriverHandler
                 GET_HEAD >=> route "/drivers" >=> getDriversHandler
                 route "/drivers" >=> OPTIONS >=> optionsDriversHandler
                 
                 GET_HEAD >=> routef "/drivers/%O" getDriverHandler
                 OPTIONS >=> routef "/drivers/%O" optionsDriverHandler


                  ]

    member __.ConfigureServices(services: IServiceCollection) =

        let titleIndex =
            FunAs.MyExpression(fun (x: Race) -> x.Title :> obj)

        let nameIndex =
            FunAs.MyExpression(fun (x: Driver) -> x.Name :> obj)

        services.AddMarten(fun x ->
            x.Connection(appConfiguration.ConnectionString)

            x.Events.InlineProjections.AggregateStreamsWith<Race>()
            |> ignore

            x.Events.InlineProjections.AggregateStreamsWith<Driver>()
            |> ignore

            x.Events.InlineProjections.Add<RaceProjection>()
            x.Events.InlineProjections.Add<PitstopProjection>()

            x
                .Schema
                .For<Race>()
                .UniqueIndex(UniqueIndexType.Computed, titleIndex)
            |> ignore

            x
                .Schema
                .For<Driver>()
                .UniqueIndex(UniqueIndexType.Computed, nameIndex)
            |> ignore)

        |> ignore

        //STJ for HAL
        let serializerOptions = JsonSerializerOptions()
        serializerOptions.Converters.Add(Hallo.Serialization.HalRepresentationConverter())
        serializerOptions.Converters.Add(Hallo.Serialization.LinksConverter())
        serializerOptions.Converters.Add(JsonFSharpConverter(JsonUnionEncoding.FSharpLuLike))
        serializerOptions.PropertyNameCaseInsensitive <- true
        serializerOptions.Encoder <- JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        services.AddSingleton(serializerOptions) |> ignore
        
        //JSON.NET for Giraffe
        let customSettings = JsonSerializerSettings()
        customSettings.Converters.Add(CompactUnionJsonConverter(true))
        services.AddSingleton<Json.ISerializer>(NewtonsoftJson.Serializer(customSettings)) |> ignore

        services.AddTransient<Hallo.Hal<RaceSummary>, RaceSummaryRepresentation>()
        |> ignore

//        services.AddTransient<Hallo.Hal<HalCars>, RaceSummaryCarsRepresentation>()
//        |> ignore
//
//        services.AddTransient<Hallo.Hal<HalCar>, RaceSummaryCarRepresentation>()
//        |> ignore

        services.AddTransient<CarRepresentation>() |> ignore
        services.AddTransient<Hal<Car>, CarRepresentation>() |> ignore
        services.AddTransient<Hal<Car array>, CarsRepresentation>() |> ignore
        
        services.AddTransient<Hallo.Hal<HalDrivers>, DriversRepresentation>()
        |> ignore
        
        services.AddTransient<Hallo.Hal<HalDriver>, DriverRepresentation>()
        |> ignore

        services.AddGiraffe() |> ignore

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =

        app.UseGiraffe webApp
