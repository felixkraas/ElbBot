using Discord.Interactions;

namespace ElbBot.App.Commands {
    public class PingCommand : InteractionModuleBase {
        [SlashCommand( "ping", "sends a ping to the bot" )]
        public async Task Ping() {
            await RespondAsync( "Pong!", ephemeral: true );
        }
    }
}