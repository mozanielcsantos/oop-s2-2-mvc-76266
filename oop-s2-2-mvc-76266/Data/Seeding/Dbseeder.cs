using Bogus;
using FoodSafety.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;

namespace OopS22Mvc76266.Web.Data.Seeding;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.MigrateAsync();

        // If data already exists, do not seed again
        if (await db.Premises.AnyAsync())
            return;

        // -----------------------------
        // 1. Seed Premises (12 total)
        // -----------------------------
        var towns = new[] { "Dublin", "Cork", "Galway" };
        var riskRatings = new[] { "Low", "Medium", "High" };

        var premisesFaker = new Faker<Premises>()
            .RuleFor(p => p.Name, f => f.Company.CompanyName())
            .RuleFor(p => p.Address, f => f.Address.StreetAddress())
            .RuleFor(p => p.Town, f => f.PickRandom(towns))
            .RuleFor(p => p.RiskRating, f => f.PickRandom(riskRatings));

        var premisesList = premisesFaker.Generate(12);

        db.Premises.AddRange(premisesList);
        await db.SaveChangesAsync();

        // -----------------------------
        // 2. Seed Inspections (25 total)
        // -----------------------------
        var inspections = new List<Inspection>();
        var random = new Random();

        for (int i = 0; i < 25; i++)
        {
            var selectedPremises = premisesList[random.Next(premisesList.Count)];
            var score = random.Next(40, 101);
            var outcome = score >= 70 ? "Pass" : "Fail";

            inspections.Add(new Inspection
            {
                PremisesId = selectedPremises.Id,
                InspectionDate = DateTime.UtcNow.AddDays(-random.Next(1, 120)),
                Score = score,
                Outcome = outcome,
                Notes = $"Inspection notes for {selectedPremises.Name}"
            });
        }

        db.Inspections.AddRange(inspections);
        await db.SaveChangesAsync();

        // -----------------------------
        // 3. Seed FollowUps (10 total)
        // -----------------------------
        var followUps = new List<FollowUp>();
        var failedInspections = inspections.Where(i => i.Outcome == "Fail").ToList();

        for (int i = 0; i < 10; i++)
        {
            var selectedInspection = failedInspections[random.Next(failedInspections.Count)];
            var dueDate = selectedInspection.InspectionDate.AddDays(random.Next(7, 30));

            bool isClosed = random.NextDouble() > 0.5;

            followUps.Add(new FollowUp
            {
                InspectionId = selectedInspection.Id,
                DueDate = dueDate,
                Status = isClosed ? "Closed" : "Open",
                ClosedDate = isClosed ? dueDate.AddDays(random.Next(1, 10)) : null
            });
        }

        db.FollowUps.AddRange(followUps);
        await db.SaveChangesAsync();
    }
}