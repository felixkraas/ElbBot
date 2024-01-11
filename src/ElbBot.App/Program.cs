using System.Reflection;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace ElbBot.App;

public class Program {
    private DiscordSocketClient _client;
    private static IConfigurationRoot _config;
    private InteractionService _interactionService;

    static Task Main( string[] args ) {
        _config = new ConfigurationBuilder()
            .AddJsonFile( "appsettings.json" )
            .AddUserSecrets<Program>()
            .Build();
        return new Program().MainAsync();
    }

    public async Task MainAsync() {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console( outputTemplate: "[{Timestamp:dd.MM.yyyy-HH:mm:ss}] [{Level:u3}] - {Message:lj}{NewLine}{Exception}" )
            .CreateLogger();

        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _interactionService = new InteractionService( _client );
        _interactionService.Log += LogAsync;

        await _client.LoginAsync( TokenType.Bot, _config["ElbBot.Discord.Token"] );
        await _client.StartAsync();

        _client.Ready += ClientOnReady;
        await Task.Delay( -1 );
    }

    private async Task ClientOnReady() {
        try {
            await _interactionService.AddModulesAsync( Assembly.GetExecutingAssembly(), null );
            foreach( var module in _interactionService.Modules ) {
                Log.Debug( "Registered Module: {Name} - {Description}", module.Name, module.Description );
            }
#if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync( 761234789209473045, deleteMissing: true );
#else
            await _interactionService.RegisterCommandsGloballyAsync();
#endif

            _client.InteractionCreated += async ( interaction ) => {
                var ctx = new SocketInteractionContext( _client, interaction );
                await _interactionService.ExecuteCommandAsync( ctx, null );
            };
        } catch( Exception ex ) {
            Log.Error( ex, "An Exception occured" );
        }
    }

    private Task LogAsync( LogMessage message ) {
        var severity = message.Severity switch {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write( severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message );
        return Task.CompletedTask;
    }
}