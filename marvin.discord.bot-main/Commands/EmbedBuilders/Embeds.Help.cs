using Discord;

namespace Marvin.Bot.Commands.EmbedBuilders;

public static partial class Embeds
{
    public static class Help
    {
        public static Embed Setup = new EmbedBuilder()
            .WithTitle("Help - Setup")
            .WithDescription("""
        To setup, follow these steps;
          
        - Use `/Register` (this will link your destiny account to your discord account).
        - Then use `/clan` setup (this will link your clan to discord)
        - **wait 15 minutes** or so whilst I scan your clan.
        - Then that's it you'll be ready to go!
          
        To set up broadcasts use `/broadcasts channel` and then use `/broadcasts manage` to manage the types of broadcasts that can come through.
      """)
            .Build();

        public static Embed Commands = new EmbedBuilder()
            .WithTitle("Help - Commands")
            .WithDescription("""
        Here is a list of commands! Example: `/leaderboard valor`
          
        **Commands**
        `/leaderboard valor`
        `/leaderboard glory`
        `/leaderboard infamy`
        `/leaderboard saint14`
        `/raid {choice}`
        `/dungeon {choice}`
        `/profile {choice}`
      """)
            .Build();

        public static Embed Broadcasts = new EmbedBuilder()
            .WithTitle("Help - Broadcasts")
            .WithDescription("""
        Broadcasts are clan achievements that are announced to the selected channel, things like titles obtained by clannies, or rare weapons like raid exotics, and even things like clan level ups.

        To enable broadcasts use `/broadcast channel` this will give you a dropdown where you can select a channel you'd like Marvin to announce them to.

        To manage which broadcasts get send use `/broadcast settings`

        If there is an item that isn't being broadcast that you want to broadcast, you can add them by using `/track`.
      """)
            .Build();

        public static Embed Announcements = new EmbedBuilder()
            .WithTitle("Help - Announcements")
            .WithDescription("""
        **Announcements are disabled globally, sadly this feature is not yet ready**

        Announcments are are daily messages that are sent to the selected channel every reset to show things like mod rotations, lost sector rotation or even Xur when he appears.

        To enable announcements use `/announcement channel` this will give you a dropdown where you can select a channel you'd like Marvin to announce them to.

        To manage which announcements get send use `/announcement manage`
      """)
            .Build();

        public static Embed Items = new EmbedBuilder()
            .WithTitle("Help - Items")
            .WithDescription(
                "This command is used to show who has any given item. You can use the item command on any profile collectible that is found in the Destiny 2 collections tab. `/item Anarchy` will return a list of people who own the anarchy item.")
            .Build();

        public static Embed Titles = new EmbedBuilder()
            .WithTitle("Help - Titles")
            .WithDescription(
                "This command is used to show who has any given title. e.g `/title Rivensbane` will return a list of people who have obtained the Rivensbane title.")
            .Build();

        public static Embed Others = new EmbedBuilder()
            .WithTitle("Help - Others")
            .WithDescription("""
        Here is a list of other commands!

        `/donate` - To donate to the bot developer, helps keep things running.
        `/request` - To request a new feature, or to report a bug.
        `/tools` - A bunch of other tools from other developers.
      """)
            .Build();
    }
}