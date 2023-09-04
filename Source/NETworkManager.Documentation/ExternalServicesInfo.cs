namespace NETworkManager.Documentation;

/// <summary>
/// Class to hold information's about an external service.
/// </summary>
public class ExternalServicesInfo : BaseInfo
{
    /// <summary>
    /// Create an instance of <see cref="ExternalServicesInfo"/> with parameters.
    /// </summary>
    /// <param name="name">Name of the external service.</param>
    /// <param name="websiteUrl">Url of the external service.</param>
    /// <param name="description">Description of the external service.</param>
    public ExternalServicesInfo(string name, string websiteUrl, string description) : base(name, websiteUrl,
        description)
    {

    }
}
