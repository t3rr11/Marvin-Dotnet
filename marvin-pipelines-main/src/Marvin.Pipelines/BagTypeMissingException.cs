namespace Marvin.Pipelines;

public class BagTypeMissingException : Exception
{
    public BagTypeMissingException(Type type) : base($"Failed to get data of type {type.Name}")
    {
    }
}