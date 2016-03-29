namespace Nrk.Odd.Oddjob

module Main = 
    open System
    open System.IO
    open System.Text
    open Serilog
    open Serilog.Context
    open Serilog.Formatting.Json;
    open Serilog.Sinks.RollingFile
    open Akka
    open Akka.FSharp

    let start () =
        System.create "MySystem" <| Configuration.load ()

    let stop (system : Actor.ActorSystem) =
        system.Dispose()

    let configureLogging (applicationName : string) =
        LogContext.PermitCrossAppDomainCalls <- true

        let writeTextLogsToApplicationFolder = false
        let logRoot = match writeTextLogsToApplicationFolder with
                      | false ->
                            let publicDir = Environment.ExpandEnvironmentVariables("%PUBLIC%")
                            let path = Path.Combine(publicDir, "MySystem", applicationName);
                            if not <| Directory.Exists(path) then Directory.CreateDirectory(path) |> ignore
                            path
                      | true -> ".\\"

        let logSink = new RollingFileSink(
                        Path.Combine(logRoot, String.Format("{0}.{{Date}}.txt", applicationName)),
                        JsonFormatter(true), Nullable(), Nullable(), UTF8Encoding(false))

        let config = LoggerConfiguration().WriteTo.ColoredConsole().MinimumLevel.Debug().WriteTo.Sink(logSink)
        let logger = config.Enrich.FromLogContext().MinimumLevel.Debug().CreateLogger()
        Serilog.Log.Logger <- logger

    [<EntryPoint>]
    let main argv = 
        configureLogging "MySystem"

        let jobHandler = start ()
        printfn "Press any key"
        Console.ReadKey() |> ignore
        stop (jobHandler)
        
        printfn "%A" argv
        0 // return an integer exit code
