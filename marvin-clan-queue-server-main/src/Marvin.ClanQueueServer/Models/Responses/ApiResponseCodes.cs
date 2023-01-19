namespace Marvin.ClanQueueServer.Models.Responses;

public enum ApiResponseCodes
{
    Success = 200,
    
    Unauthorized = 401,
    Forbidden = 403,
    
    InternalError = 500
}