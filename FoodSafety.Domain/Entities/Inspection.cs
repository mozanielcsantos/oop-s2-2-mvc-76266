namespace FoodSafety.Domain.Entities;

public class Inspection
{
    public int Id { get; set; }

    public int PremisesId { get; set; }
    public DateTime InspectionDate { get; set; }
    public int Score { get; set; }
    public string Outcome { get; set; } = "";
    public string Notes { get; set; } = "";

    public Premises? Premises { get; set; }
    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
}