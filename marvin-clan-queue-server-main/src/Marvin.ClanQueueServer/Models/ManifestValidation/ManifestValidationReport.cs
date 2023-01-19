namespace Marvin.ClanQueueServer.Models.ManifestValidation;

public class ManifestValidationReport
{
    public List<ManifestValidationError> Errors { get; }
    public List<ManifestValidationWarning> Warnings { get; }
    public List<ManifestValidationSuccess> Successes { get; }

    public ManifestValidationReport()
    {
        Errors = new List<ManifestValidationError>();
        Warnings = new List<ManifestValidationWarning>();
        Successes = new List<ManifestValidationSuccess>();
    }
}