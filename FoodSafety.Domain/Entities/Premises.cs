namespace FoodSafety.Domain.Entities;

public class Premises
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Town { get; set; } = "";
    public string RiskRating { get; set; } = "";

    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
}