using DotNetBungieAPI.Models.Destiny;

namespace Marvin.DbAccess.Models.User;

public class UserItemCollectible
{
    /// <summary>
    ///     State of this item
    /// </summary>
    public DestinyCollectibleState State { get; set; }
}