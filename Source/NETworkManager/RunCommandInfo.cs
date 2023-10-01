namespace NETworkManager;

public class RunCommandInfo
{
    public RunCommandType Type { get; set; }
    
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Translated name.
    /// </summary>
    public string TranslatedName { get; set; }
    
    /// <summary>
    /// Command to run.
    /// </summary>
    public string Command { get; set; }
    
    /// <summary>
    /// Indicates whether (optional) arguments can be passed.
    /// </summary>
    public bool CanHandleArguments { get; set; }
    
    /// <summary>
    /// Example arguments.
    /// </summary>
    public string ExampleArguments { get; set; }

    public override string ToString()
    {
        return $"{Command}";
    }
}