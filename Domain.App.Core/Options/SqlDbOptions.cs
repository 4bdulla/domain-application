namespace Domain.App.Core.Options;

public class SqlDbOptions
{
    public string ConnectionString { get; set; }
    public bool CreateDbInDevelopmentEnvironment { get; set; } = true;
}