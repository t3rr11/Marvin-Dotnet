using Discord;

namespace Marvin.Bot.Commands.EmbedBuilders;

public static partial class Embeds
{
    public static class Clan
    {
        public static Embed Help { get; } = new EmbedBuilder()
            .WithTitle("Clan - Help")
            .WithDescription(
                """ 
                **Setup** - It's easy! 
                - First register by using the `/register` command. 
                - Then once registered `/clan setup`  
                
                **Want more than one clan?** 
                Todo 
                
                **Manage** 
                Todo  
                
                **Remove** 
                Todo 
                """)
            .Build();
    }
}