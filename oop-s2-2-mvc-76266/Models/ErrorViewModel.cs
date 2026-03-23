namespace OopS22Mvc76266.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string FriendlyMessage { get; set; } = "Sorry, something went wrong while processing your request.";
}
