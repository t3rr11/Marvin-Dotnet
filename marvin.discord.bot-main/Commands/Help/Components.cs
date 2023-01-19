using Discord;

namespace Marvin.Bot.Commands.Help;

public class Components
{
    public static MessageComponent components = new ComponentBuilder()
      .WithButton(label: "Setup", customId: Events.SetupBtnId, style: ButtonStyle.Primary, row: 0)
      .WithButton(label: "Commands", customId: Events.CommandsBtnId, style: ButtonStyle.Primary, row: 0)
      .WithButton(label: "Broadcasts", customId: Events.BroadcastsBtnId, style: ButtonStyle.Primary, row: 0)
      .WithButton(label: "Announcements", customId: Events.AnnouncementsBtnId, style: ButtonStyle.Primary, row: 0)
      .WithButton(label: "Items", customId: Events.ItemsBtnId, style: ButtonStyle.Primary, row: 1)
      .WithButton(label: "Titles", customId: Events.TitlesBtnId, style: ButtonStyle.Primary, row: 1)
      .WithButton(label: "Others", customId: Events.OthersBtnId, style: ButtonStyle.Primary, row: 1)
      .WithButton(label: "View all commands", customId: Events.ViewAllCommandsBtnId, style: ButtonStyle.Secondary, row: 2)
      .Build();
}
