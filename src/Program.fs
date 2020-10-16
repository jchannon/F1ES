open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open F1ES


[<EntryPoint>]
let main argv =
    
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseStartup<Startup>()
                    |> ignore)
        .Build()
        .Run()
    0
