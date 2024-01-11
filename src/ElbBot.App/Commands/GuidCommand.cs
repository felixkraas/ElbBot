using System.Text;
using Discord;
using Discord.Interactions;

namespace ElbBot.App.Commands {
    public class GuidCommand : InteractionModuleBase<SocketInteractionContext> {
        [SlashCommand( "guid", "Generates a new GUID" )]
        public async Task NewGuid( [MinValue( 1 ), MaxValue( 25 )] int amount = 1, GuidFormat format = GuidFormat.N ) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "Some freshly baked GUID(s) for you:" );
            sb.Append( "```" );

            for( int i = 0; i <= amount; i++ ) {
                sb.AppendLine( Guid.NewGuid().ToString( format.ToString() ) );
            }

            sb.Append( "```" );

            await RespondAsync( sb.ToString(), ephemeral: true );
        }

        public enum GuidFormat {
            N,
            D,
            B,
            P,
            X
        }
    }
}