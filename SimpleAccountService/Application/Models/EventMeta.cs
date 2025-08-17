namespace Simple_Account_Service.Application.Models;

public class EventMeta
{
    public string Version { get; set; } = "";
    public string Source { get; set; } = "";
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}