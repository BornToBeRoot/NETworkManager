namespace NETworkManager.Models;

public class RunCommandInfo
{
    /// <summary>
    /// Name of the application.
    /// </summary>
    public ApplicationName Name { get; set; }
    
    /// <summary>
    /// Command to run the application.
    /// </summary>
    public string Command { get; set; }
    
    /// <summary>
    /// Indicates whether the application can be run with arguments.
    /// </summary>
    public bool CanHandleArguments { get; set; }
    
    /// <summary>
    /// Example arguments for the application.
    /// </summary>
    public string ExampleArguments { get; set; }
}