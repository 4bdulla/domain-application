namespace Domain.App.Core.Options;

public class ClientOptions
{
    public string BaseAddress { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
}