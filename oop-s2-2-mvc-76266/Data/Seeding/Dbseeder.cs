using FoodSafety.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OopS22Mvc76266.Web.Data.Seeding
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.Premises.AnyAsync())
            {
                return;
            }

            var premises = new List<Premises>
            {
                new Premises { Name = "River Cafe", Address = "1 Main Street", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "Oak Bakery", Address = "12 Green Road", Town = "Dublin", RiskRating = "Medium" },
                new Premises { Name = "City Deli", Address = "45 Market Lane", Town = "Dublin", RiskRating = "Low" },
                new Premises { Name = "Harbour Grill", Address = "3 Dock Street", Town = "Cork", RiskRating = "High" },
                new Premises { Name = "Sunrise Bistro", Address = "9 Bridge Road", Town = "Cork", RiskRating = "Medium" },
                new Premises { Name = "Central Kitchen", Address = "22 King Street", Town = "Cork", RiskRating = "Low" },
                new Premises { Name = "Hilltop Eatery", Address = "7 Castle Avenue", Town = "Galway", RiskRating = "High" },
                new Premises { Name = "Westside Cafe", Address = "14 Quay Street", Town = "Galway", RiskRating = "Medium" },
                new Premises { Name = "Sea Breeze Foods", Address = "5 Harbour View", Town = "Galway", RiskRating = "Low" },
                new Premises { Name = "Elm Restaurant", Address = "30 Station Road", Town = "Dublin", RiskRating = "High" },
                new Premises { Name = "Northside Bakery", Address = "16 College Park", Town = "Cork", RiskRating = "Medium" },
                new Premises { Name = "Lakeside Diner", Address = "28 Riverside Walk", Town = "Galway", RiskRating = "Low" }
            };

            db.Premises.AddRange(premises);
            await db.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var currentMonth = new DateTime(now.Year, now.Month, 1);

            var inspections = new List<Inspection>
            {
                new Inspection { PremisesId = premises[0].Id, InspectionDate = currentMonth.AddDays(1), Score = 48, Outcome = "Fail", Notes = "Temperature control issue" },
                new Inspection { PremisesId = premises[1].Id, InspectionDate = currentMonth.AddDays(2), Score = 78, Outcome = "Pass", Notes = "Minor issues noted" },
                new Inspection { PremisesId = premises[2].Id, InspectionDate = currentMonth.AddDays(3), Score = 88, Outcome = "Pass", Notes = "Clean and compliant" },
                new Inspection { PremisesId = premises[3].Id, InspectionDate = currentMonth.AddDays(4), Score = 41, Outcome = "Fail", Notes = "Poor cleaning schedule" },
                new Inspection { PremisesId = premises[4].Id, InspectionDate = currentMonth.AddDays(5), Score = 67, Outcome = "Pass", Notes = "Acceptable standard" },
                new Inspection { PremisesId = premises[5].Id, InspectionDate = currentMonth.AddDays(6), Score = 55, Outcome = "Fail", Notes = "Cross-contamination risk" },
                new Inspection { PremisesId = premises[6].Id, InspectionDate = currentMonth.AddDays(7), Score = 82, Outcome = "Pass", Notes = "Well managed operation" },
                new Inspection { PremisesId = premises[7].Id, InspectionDate = currentMonth.AddDays(8), Score = 61, Outcome = "Pass", Notes = "Borderline pass" },
                new Inspection { PremisesId = premises[8].Id, InspectionDate = currentMonth.AddDays(9), Score = 35, Outcome = "Fail", Notes = "Storage failure" },
                new Inspection { PremisesId = premises[9].Id, InspectionDate = currentMonth.AddDays(10), Score = 90, Outcome = "Pass", Notes = "Strong compliance" },
                new Inspection { PremisesId = premises[10].Id, InspectionDate = currentMonth.AddDays(11), Score = 72, Outcome = "Pass", Notes = "Good standard" },
                new Inspection { PremisesId = premises[11].Id, InspectionDate = currentMonth.AddDays(12), Score = 58, Outcome = "Fail", Notes = "Documentation incomplete" },

                new Inspection { PremisesId = premises[0].Id, InspectionDate = currentMonth.AddMonths(-1).AddDays(2), Score = 64, Outcome = "Pass", Notes = "Previous month follow-up complete" },
                new Inspection { PremisesId = premises[1].Id, InspectionDate = currentMonth.AddMonths(-1).AddDays(4), Score = 52, Outcome = "Fail", Notes = "Old inspection" },
                new Inspection { PremisesId = premises[2].Id, InspectionDate = currentMonth.AddMonths(-1).AddDays(6), Score = 77, Outcome = "Pass", Notes = "Previous audit pass" },
                new Inspection { PremisesId = premises[3].Id, InspectionDate = currentMonth.AddMonths(-1).AddDays(8), Score = 49, Outcome = "Fail", Notes = "Repeat hygiene issue" },
                new Inspection { PremisesId = premises[4].Id, InspectionDate = currentMonth.AddMonths(-1).AddDays(10), Score = 81, Outcome = "Pass", Notes = "Stable compliance" },
                new Inspection { PremisesId = premises[5].Id, InspectionDate = currentMonth.AddMonths(-1).AddDays(12), Score = 60, Outcome = "Pass", Notes = "Pass threshold met" },
                new Inspection { PremisesId = premises[6].Id, InspectionDate = currentMonth.AddMonths(-2).AddDays(3), Score = 46, Outcome = "Fail", Notes = "Old critical issue" },
                new Inspection { PremisesId = premises[7].Id, InspectionDate = currentMonth.AddMonths(-2).AddDays(5), Score = 86, Outcome = "Pass", Notes = "Good outcome" },
                new Inspection { PremisesId = premises[8].Id, InspectionDate = currentMonth.AddMonths(-2).AddDays(7), Score = 73, Outcome = "Pass", Notes = "Routine inspection" },
                new Inspection { PremisesId = premises[9].Id, InspectionDate = currentMonth.AddMonths(-2).AddDays(9), Score = 57, Outcome = "Fail", Notes = "Sanitation issue" },
                new Inspection { PremisesId = premises[10].Id, InspectionDate = currentMonth.AddMonths(-2).AddDays(11), Score = 68, Outcome = "Pass", Notes = "Improvement noted" },
                new Inspection { PremisesId = premises[11].Id, InspectionDate = currentMonth.AddMonths(-2).AddDays(13), Score = 44, Outcome = "Fail", Notes = "Refrigeration problem" },
                new Inspection { PremisesId = premises[4].Id, InspectionDate = currentMonth.AddMonths(-3).AddDays(6), Score = 83, Outcome = "Pass", Notes = "Historic pass" }
            };

            db.Inspections.AddRange(inspections);
            await db.SaveChangesAsync();

            var followUps = new List<FollowUp>
            {
                new FollowUp { InspectionId = inspections[0].Id, DueDate = now.AddDays(-5), Status = "Open", ClosedDate = null },
                new FollowUp { InspectionId = inspections[3].Id, DueDate = now.AddDays(-2), Status = "Open", ClosedDate = null },
                new FollowUp { InspectionId = inspections[5].Id, DueDate = now.AddDays(3), Status = "Open", ClosedDate = null },
                new FollowUp { InspectionId = inspections[8].Id, DueDate = now.AddDays(-7), Status = "Open", ClosedDate = null },
                new FollowUp { InspectionId = inspections[11].Id, DueDate = now.AddDays(5), Status = "Open", ClosedDate = null },

                new FollowUp { InspectionId = inspections[13].Id, DueDate = now.AddDays(-20), Status = "Closed", ClosedDate = now.AddDays(-10) },
                new FollowUp { InspectionId = inspections[15].Id, DueDate = now.AddDays(-14), Status = "Closed", ClosedDate = now.AddDays(-6) },
                new FollowUp { InspectionId = inspections[18].Id, DueDate = now.AddDays(-9), Status = "Closed", ClosedDate = now.AddDays(-4) },
                new FollowUp { InspectionId = inspections[21].Id, DueDate = now.AddDays(-11), Status = "Closed", ClosedDate = now.AddDays(-3) },
                new FollowUp { InspectionId = inspections[23].Id, DueDate = now.AddDays(-16), Status = "Closed", ClosedDate = now.AddDays(-8) }
            };

            db.FollowUps.AddRange(followUps);
            await db.SaveChangesAsync();
        }
    }
}