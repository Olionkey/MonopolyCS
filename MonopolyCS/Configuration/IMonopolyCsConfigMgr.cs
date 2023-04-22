using MonopolyCS.Configuration.Sections;

namespace MonopolyCS.Configuration;

public interface IMonopolyCsConfigMgr
{
    EnvironmentVariables EnvironmentVariables { get; }
}