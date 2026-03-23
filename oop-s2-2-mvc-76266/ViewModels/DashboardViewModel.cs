namespace OopS22Mvc76266.Web.ViewModels;

public class DashboardViewModel
{
    public string? Town { get; set; }
    public string? RiskRating { get; set; }

    public List<string> AvailableTowns { get; set; } = new();
    public List<string> AvailableRiskRatings { get; set; } = new();

    public int InspectionsThisMonth { get; set; }
    public int FailedInspectionsThisMonth { get; set; }
    public int OverdueOpenFollowUps { get; set; }
}