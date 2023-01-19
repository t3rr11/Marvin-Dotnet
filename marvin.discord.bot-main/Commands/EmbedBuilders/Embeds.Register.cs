using Discord;

namespace Marvin.Bot.Commands.Register
{
    public static partial class Embeds
    {
        public static class Register
        {
            public static Embed MultipleResults()
            {
                return new EmbedBuilder()
                    .WithTitle("Please pick a user")
                    .WithDescription("Looks like your name is quite popular, here are the ones we found.")
                    .WithFooter("Marvin.gg", "https://marvin.gg/assets/images/logo.png")
                    .WithCurrentTimestamp()
                    .Build();
            }

            public static Embed NoResults(string name)
            {
                return new EmbedBuilder()
                    .WithTitle("Unable to find account")
                    .WithDescription($"""
               Could not find a Destiny 2 account that matches: { name} . Please double check try again.
               
               **Hint**
               Bungie given names which look a little something like this. Marvin#1234 usually have more success.
               """ )
                    .WithFooter("Marvin.gg", "https://marvin.gg/assets/images/logo.png")
                    .WithCurrentTimestamp()
                    .Build();
            }

            public static Embed TooManyResults()
            {
                return new EmbedBuilder()
                    .WithTitle("Too many results")
                    .WithDescription(
                        "There were too many results to display. Please use something more unique like your bungie name. Example: Marvin#1234")
                    .WithFooter("Marvin.gg", "https://marvin.gg/assets/images/logo.png")
                    .WithCurrentTimestamp()
                    .Build();
            }

            public static Embed SuccessfullyRegistered(string name)
            {
                return new EmbedBuilder()
                    .WithTitle("Successfully registered")
                    .WithDescription($"Your username has been set to: {name}.")
                    .WithFooter("Marvin.gg", "https://marvin.gg/assets/images/logo.png")
                    .WithCurrentTimestamp()
                    .Build();
            }
        }
    }
}