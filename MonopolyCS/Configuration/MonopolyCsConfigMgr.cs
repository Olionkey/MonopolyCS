using MonopolyCS.Configuration.Sections;

namespace MonopolyCS.Configuration;

public class MonopolyCsConfigMgr : IMonopolyCsConfigMgr
{
    public EnvironmentVariables EnvironmentVariables { get; set; } = new();
}