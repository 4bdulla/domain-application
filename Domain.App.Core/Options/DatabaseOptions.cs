namespace Domain.App.Core.Options;

public class DatabaseOptions
{
    public string ConnectionString { get; set; }
    public bool CreateDbInDevelopmentEnvironment { get; set; } = true;
}