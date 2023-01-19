namespace Marvin.ClanScannerServer.Extensions;

public static class BooleanExtensions
{
    public static int ToInt32(this bool value) => value switch
    {
        false => 0, 
        true => 1
    };
}