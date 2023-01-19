namespace Marvin.DbAccess.Models.Announcing;

public class AnnouncementData
{
    public Guid Id { get; set; }
    public string Body { get; set; }
    public AnnouncementType Type { get; set; }
    public DateTime DateReported { get; set; }
}