using FoodSafety.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OopS22Mvc76266.Web.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Premises> Premises => Set<Premises>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Premises>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Address).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Town).IsRequired().HasMaxLength(100);
            entity.Property(p => p.RiskRating).IsRequired().HasMaxLength(20);
        });

        builder.Entity<Inspection>(entity =>
        {
            entity.Property(i => i.Score).IsRequired();
            entity.Property(i => i.Outcome).IsRequired().HasMaxLength(20);
            entity.Property(i => i.Notes).HasMaxLength(500);

            entity.HasOne(i => i.Premises)
                .WithMany(p => p.Inspections)
                .HasForeignKey(i => i.PremisesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FollowUp>(entity =>
        {
            entity.Property(f => f.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(f => f.Inspection)
                .WithMany(i => i.FollowUps)
                .HasForeignKey(f => f.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}