namespace Domain.App.Core.Database.Abstractions;

public interface IAuditable
{
    string Creator { get; set; }
    DateTime CreatedAt { get; set; }
    string Modifier { get; set; }
    DateTime? ModifiedAt { get; set; }
}