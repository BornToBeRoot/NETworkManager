namespace NETworkManager.Models;

public class RunCommandInfo
{
    public ApplicationName Name { get; set; }
    
    public string Command { get; set; }
    
    public bool CanHandleArguments { get; set; }
    
    public string ExampleArguments { get; set; }
}