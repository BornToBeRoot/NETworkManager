namespace NETworkManager;

public class RunCommandInfo
{
    public RunCommandType Type { get; init; }
    
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    /// Translated name.
    /// </summary>
    public string TranslatedName { get; init; }
    
    /// <summary>
    /// Command to run.
    /// </summary>
    public string Command { get; set; }
    
    /// <summary>
    /// Indicates whether (optional) arguments can be passed.
    /// </summary>
    public bool CanHandleArguments { get; init; }
    
    /// <summary>
    /// Example arguments.
    /// </summary>
    public string ExampleArguments { get; set; }

    public override string ToString()
    {
        return $"{Command}";
    }
}