module App

open System
open System.IO
open System.Text
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open Giraffe
open Features

// Web app
// ---------------------------------

let authorize : HttpFunc -> HttpContext -> HttpFuncResult =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> text "Web API started." ]
        POST >=>
            choose [
                route "/accounts/getToken" >=> getToken
                route "/accounts/register" >=> register ]
        setStatusCode 404 >=> text "Not Found" ]

// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// Config
// ---------------------------------

let configureApp (app : IApplicationBuilder) =
    app
       .UseAuthentication()
       .UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseGiraffe webApp
       
let configureAppConfiguration (ctx: WebHostBuilderContext) (cfg: IConfigurationBuilder) =  
    cfg
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile(sprintf "appsettings.%s.json" ctx.HostingEnvironment.EnvironmentName, true)
        .AddEnvironmentVariables()
        |> ignore

let configureServices (services : IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let config = serviceProvider.GetService<IConfiguration>()
    let secret = string <| config.GetValue "secret"
    
    let authenticationOptions (o : AuthenticationOptions) =
        o.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
        o.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme
        
    let jwtBearerOptions (cfg : JwtBearerOptions) =
        cfg.TokenValidationParameters <- TokenValidationParameters (
            ValidateActor = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "fsharp.org",
            ValidAudience = "fsharp.org",
            IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        )
        cfg.RequireHttpsMetadata <- false

    services
        .AddGiraffe()
        .AddAuthentication(authenticationOptions)
        .AddJwtBearer(Action<JwtBearerOptions> jwtBearerOptions) |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore
    
// Main
// ---------------------------------

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseKestrel()
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(configureApp)
                    .ConfigureAppConfiguration(configureAppConfiguration)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0