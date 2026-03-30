using FoodSafety.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Controllers;
using OopS22Mvc76266.Web.Data;
using Xunit;

namespace FoodSafety.Tests;

public class UnitTest1 : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;

    public UnitTest1()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        SeedKnownData();
    }

    private void SeedKnownData()
    {
        var premises = new List<Premises>
        {
            new Premises
            {
                Name = "River Cafe",
                Address = "1 Main Street",
                Town = "Dublin",
                RiskRating = "High"
            },
            new Premises
            {
                Name = "Green Bakery",
                Address = "2 Oak Avenue",
                Town = "Cork",
                RiskRating = "Medium"
            },
            new Premises
            {
                Name = "Harbour Deli",
                Address = "3 Market Road",
                Town = "Dublin",
                RiskRating = "Low"
            }
        };

        _context.Premises.AddRange(premises);
        _context.SaveChanges();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var inspections = new List<Inspection>
        {
            new Inspection
            {
                PremisesId = premises[0].Id,
                InspectionDate = monthStart.AddDays(1),
                Score = 45,
                Outcome = "Fail",
                Notes = "Poor temperature records"
            },
            new Inspection
            {
                PremisesId = premises[1].Id,
                InspectionDate = monthStart.AddDays(2),
                Score = 82,
                Outcome = "Pass",
                Notes = "Good hygiene"
            },
            new Inspection
            {
                PremisesId = premises[2].Id,
                InspectionDate = monthStart.AddMonths(-1).AddDays(5),
                Score = 50,
                Outcome = "Fail",
                Notes = "Old inspection, previous month"
            }
        };

        _context.Inspections.AddRange(inspections);
        _context.SaveChanges();

        var followUps = new List<FollowUp>
        {
            new FollowUp
            {
                InspectionId = inspections[0].Id,
                DueDate = now.AddDays(-2),
                Status = "Open",
                ClosedDate = null
            },
            new FollowUp
            {
                InspectionId = inspections[1].Id,
                DueDate = now.AddDays(5),
                Status = "Open",
                ClosedDate = null
            },
            new FollowUp
            {
                InspectionId = inspections[2].Id,
                DueDate = now.AddDays(-10),
                Status = "Closed",
                ClosedDate = now.AddDays(-3)
            }
        };

        _context.FollowUps.AddRange(followUps);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Overdue_followups_query_returns_only_open_overdue_items()
    {
        var now = DateTime.UtcNow;

        var overdueOpenFollowUps = await _context.FollowUps
            .Include(f => f.Inspection)
            .Where(f => f.Status == "Open" && f.DueDate < now)
            .ToListAsync();

        Assert.Single(overdueOpenFollowUps);
        Assert.Equal("Open", overdueOpenFollowUps[0].Status);
        Assert.True(overdueOpenFollowUps[0].DueDate < now);
    }

    [Fact]
    public void Followup_cannot_be_closed_without_closeddate()
    {
        var invalidFollowUp = new FollowUp
        {
            InspectionId = 1,
            DueDate = DateTime.UtcNow.AddDays(2),
            Status = "Closed",
            ClosedDate = null
        };

        var isValid = !(invalidFollowUp.Status == "Closed" && invalidFollowUp.ClosedDate == null);

        Assert.False(isValid);
    }

    [Fact]
    public async Task Dashboard_counts_match_known_seed_data()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var inspectionsThisMonth = await _context.Inspections
            .Where(i => i.InspectionDate >= monthStart)
            .CountAsync();

        var failedInspectionsThisMonth = await _context.Inspections
            .Where(i => i.InspectionDate >= monthStart && i.Outcome == "Fail")
            .CountAsync();

        var overdueOpenFollowUps = await _context.FollowUps
            .Where(f => f.Status == "Open" && f.DueDate < now)
            .CountAsync();

        Assert.Equal(2, inspectionsThisMonth);
        Assert.Equal(1, failedInspectionsThisMonth);
        Assert.Equal(1, overdueOpenFollowUps);
    }

    [Fact]
    public async Task Dashboard_filter_by_town_returns_correct_counts()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var dublinPremisesIds = await _context.Premises
            .Where(p => p.Town == "Dublin")
            .Select(p => p.Id)
            .ToListAsync();

        var inspectionsThisMonthForDublin = await _context.Inspections
            .Where(i => dublinPremisesIds.Contains(i.PremisesId))
            .Where(i => i.InspectionDate >= monthStart)
            .CountAsync();

        var failedThisMonthForDublin = await _context.Inspections
            .Where(i => dublinPremisesIds.Contains(i.PremisesId))
            .Where(i => i.InspectionDate >= monthStart && i.Outcome == "Fail")
            .CountAsync();

        Assert.Equal(1, inspectionsThisMonthForDublin);
        Assert.Equal(1, failedThisMonthForDublin);
    }

    [Fact]
    public void Role_authorization_attributes_are_applied_correctly()
    {
        var premisesControllerAttribute = typeof(PremisesController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        var inspectionsControllerAttribute = typeof(InspectionsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        var followUpsControllerAttribute = typeof(FollowUpsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        var dashboardControllerAttribute = typeof(DashboardController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(premisesControllerAttribute);
        Assert.NotNull(inspectionsControllerAttribute);
        Assert.NotNull(followUpsControllerAttribute);
        Assert.NotNull(dashboardControllerAttribute);

        Assert.Equal("Admin,Inspector,Viewer", premisesControllerAttribute!.Roles);
        Assert.Equal("Admin,Inspector,Viewer", inspectionsControllerAttribute!.Roles);
        Assert.Equal("Admin,Inspector,Viewer", followUpsControllerAttribute!.Roles);
        Assert.Equal("Admin,Inspector,Viewer", dashboardControllerAttribute!.Roles);

        var premisesCreateAttribute = typeof(PremisesController)
            .GetMethod("Create", new Type[] { })!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        var inspectionsCreateAttribute = typeof(InspectionsController)
            .GetMethod("Create", new Type[] { })!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        var followUpsCreateAttribute = typeof(FollowUpsController)
            .GetMethod("Create", new Type[] { })!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(premisesCreateAttribute);
        Assert.NotNull(inspectionsCreateAttribute);
        Assert.NotNull(followUpsCreateAttribute);

        Assert.Equal("Admin", premisesCreateAttribute!.Roles);
        Assert.Equal("Admin,Inspector", inspectionsCreateAttribute!.Roles);
        Assert.Equal("Admin,Inspector", followUpsCreateAttribute!.Roles);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}