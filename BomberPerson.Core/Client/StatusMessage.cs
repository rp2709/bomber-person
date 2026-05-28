namespace BomberPerson.Core.Client;

public record StatusMessage(string Message,StatusMessage.ImportanceLevels Importance )
{
    public enum ImportanceLevels
    {
        Success,
        Info,
        Warning,
        Error
    }
}