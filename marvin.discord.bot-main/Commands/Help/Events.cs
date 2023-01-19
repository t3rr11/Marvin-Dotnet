using Discord.Interactions;
using Discord.WebSocket;
using Marvin.Bot.Commands.EmbedBuilders;

namespace Marvin.Bot.Commands.Help;

public class Events : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    public const string SetupBtnId = "setup-btn";
    public const string CommandsBtnId = "commands-btn";
    public const string BroadcastsBtnId = "broadcasts-btn";
    public const string AnnouncementsBtnId = "announcements-btn";
    public const string ItemsBtnId = "items-btn";
    public const string TitlesBtnId = "titles-btn";
    public const string OthersBtnId = "others-btn";
    public const string ViewAllCommandsBtnId = "view-all-commands-btn";

    [ComponentInteraction(SetupBtnId)]
    public async Task OnSetupButtonClick()
    {
        await Context.Interaction.DeferAsync(ephemeral: true);
        try
        {
            await Context.Interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = Embeds.Help.Setup;
                message.Components = Components.components;
            });
        }
        catch (Exception ex)
        {
            // TODO: Add generic error embed response on failure.
            Console.WriteLine(ex.ToString());
        }
    }

    [ComponentInteraction(CommandsBtnId)]
    public async Task OnCommandsButtonClick()
    {
        await Context.Interaction.DeferAsync(ephemeral: true);
        try
        {
            await Context.Interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = Embeds.Help.Commands;
                message.Components = Components.components;
            });
        }
        catch (Exception ex)
        {
            // TODO: Add generic error embed response on failure.
            Console.WriteLine(ex.ToString());
        }
    }

    [ComponentInteraction(BroadcastsBtnId)]
    public async Task OnBroadcastsButtonClick()
    {
        await Context.Interaction.DeferAsync(ephemeral: true);
        try
        {
            await Context.Interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = Embeds.Help.Broadcasts;
                message.Components = Components.components;
            });
        }
        catch (Exception ex)
        {
            // TODO: Add generic error embed response on failure.
            Console.WriteLine(ex.ToString());
        }
    }

    [ComponentInteraction(AnnouncementsBtnId)]
    public async Task OnAnnouncementsButtonClick()
    {
        await Context.Interaction.DeferAsync(ephemeral: true);
        try
        {
            await Context.Interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = Embeds.Help.Announcements;
                message.Components = Components.components;
            });
        }
        catch (Exception ex)
        {
            // TODO: Add generic error embed response on failure.
            Console.WriteLine(ex.ToString());
        }
    }

    [ComponentInteraction(ItemsBtnId)]
    public async Task OnItemsButtonClick()
    {
        await Context.Interaction.DeferAsync(ephemeral: true);
        try
        {
            await Context.Interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = Embeds.Help.Items;
                message.Components = Components.components;
            });
        }
        catch (Exception ex)
        {
            // TODO: Add generic error embed response on failure.
            Console.WriteLine(ex.ToString());
        }
    }

    [ComponentInteraction(TitlesBtnId)]
    public async Task OnTitlesButtonClick()
    {
        await Context.Interaction.DeferAsync(ephemeral: true);
        try
        {
            await Context.Interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = Embeds.Help.Titles;
                message.Components = Components.components;
            });
        }
        catch (Exception ex)
        {
            // TODO: Add generic error embed response on failure.
            Console.WriteLine(ex.ToString());
        }
    }

    [ComponentInteraction(OthersBtnId)]
    public async Task OnOthersButtonClick()
    {
        await Context.Interaction.DeferAsync(ephemeral: true);
        try
        {
            await Context.Interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = Embeds.Help.Others;
                message.Components = Components.components;
            });
        }
        catch (Exception ex)
        {
            // TODO: Add generic error embed response on failure.
            Console.WriteLine(ex.ToString());
        }
    }
}