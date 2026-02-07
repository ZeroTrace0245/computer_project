using Microsoft.EntityFrameworkCore;
using computer_project.ApiService.Models;

namespace computer_project.ApiService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<HealthReport> HealthReports => Set<HealthReport>();
    public DbSet<AIRecommendation> AIRecommendations => Set<AIRecommendation>();
    public DbSet<WaterIntake> WaterIntakes => Set<WaterIntake>();
    public DbSet<UserGoal> UserGoals => Set<UserGoal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Seed some data for demo purposes
        modelBuilder.Entity<User>().HasData(new User { Id = 1, Username = "admin", PasswordHash = "admin", Role = "Admin" });
        modelBuilder.Entity<UserGoal>().HasData(new UserGoal 
        { 
            Id = 1, 
            UserId = 1, 
            TargetCalories = 2000, 
            TargetProtein = 150, 
            TargetCarbs = 200, 
            TargetFat = 70, 
            TargetWater = 2.5 
        });
    }
}

