using computer_project.ApiService.Data;
using computer_project.ApiService.Models;
using computer_project.ApiService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<AIService>();

// Use In-Memory for now
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("SmartBite"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Users.Any())
    {
        db.Users.Add(new User { Username = "user", PasswordHash = "password", Role = "EndUser" });
        db.Users.Add(new User { Username = "admin", PasswordHash = "admin", Role = "Admin" });
        db.UserGoals.Add(new UserGoal 
        { 
            UserId = 1, 
            TargetCalories = 2000, 
            TargetProtein = 150, 
            TargetCarbs = 200, 
            TargetFat = 65, 
            TargetWater = 2.5 
        });
        db.SaveChanges();
    }
}

// --- Endpoints ---

// User
app.MapGet("/users/{id}", async (int id, AppDbContext db) => 
    await db.Users.FindAsync(id) is User user ? Results.Ok(user) : Results.NotFound());

app.MapPost("/users", async (UserRegistrationRequest request, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Username == request.Username))
        return Results.BadRequest("Username already exists");

    var user = new User { 
        Username = request.Username, 
        PasswordHash = request.Password // Simple demo, no hashing
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPost("/login", async (LoginRequest request, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);
    return user is not null ? Results.Ok(user) : Results.Unauthorized();
});

// Meals
app.MapGet("/meals", async (AppDbContext db) => 
    await db.Meals.OrderByDescending(m => m.LoggedAt).ToListAsync());

app.MapPost("/meals", async (Meal meal, AppDbContext db) =>
{
    db.Meals.Add(meal);
    await db.SaveChangesAsync();
    return Results.Created($"/meals/{meal.Id}", meal);
});

// Shopping List
app.MapGet("/shoppinglist", async (AppDbContext db) => 
    await db.ShoppingListItems.ToListAsync());

app.MapPost("/shoppinglist", async (ShoppingListItem item, AppDbContext db) =>
{
    db.ShoppingListItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/shoppinglist/{item.Id}", item);
});

app.MapPut("/shoppinglist/{id}", async (int id, ShoppingListItem inputItem, AppDbContext db) =>
{
    var item = await db.ShoppingListItems.FindAsync(id);
    if (item is null) return Results.NotFound();
    
    item.IsPurchased = inputItem.IsPurchased;
    item.ItemName = inputItem.ItemName;
    item.Quantity = inputItem.Quantity;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Stats / Reports
app.MapGet("/stats", async (AppDbContext db, AIService ai) => 
{
    var meals = await db.Meals.ToListAsync();
    var goal = await db.UserGoals.FirstOrDefaultAsync(g => g.UserId == 1);

    var report = new HealthReport {
        UserId = 1, // Default for demo
        TotalCalories = meals.Sum(m => m.Calories),
        MealCount = meals.Count,
        Protein = meals.Sum(m => m.Protein),
        Carbs = meals.Sum(m => m.Carbs),
        Fat = meals.Sum(m => m.Fat),
        GeneratedAt = DateTime.UtcNow
    };

    report.Summary = await ai.GenerateHealthReportAsync(report, goal);
    return report;
});

// AI Recommendations
app.MapGet("/recommendations", async (AppDbContext db, AIService ai) => 
{
    var recentMeals = await db.Meals.OrderByDescending(m => m.LoggedAt).Take(10).ToListAsync();
    var goal = await db.UserGoals.FirstOrDefaultAsync(g => g.UserId == 1);
    return await ai.GetRecommendationsAsync(recentMeals, goal);
});

app.MapPost("/ai/chat", async (ChatRequest request, AIService ai) =>
{
    var response = await ai.GetChatResponseAsync(request.Prompt);
    return Results.Ok(response);
});

app.MapGet("/ai/estimate", async (string mealDescription, AIService ai) =>
{
    var estimation = await ai.EstimateNutritionAsync(mealDescription);
    return estimation is not null ? Results.Ok(estimation) : Results.NotFound();
});

// Water Tracking
app.MapGet("/water", async (AppDbContext db) => 
    await db.WaterIntakes.OrderByDescending(w => w.LoggedAt).ToListAsync());

app.MapGet("/water/advice", async (double current, double target, AIService ai) => 
{
    return await ai.GetWaterAdviceAsync(current, target);
});

app.MapPost("/water", async (WaterIntake intake, AppDbContext db) =>
{
    db.WaterIntakes.Add(intake);
    await db.SaveChangesAsync();
    return Results.Created($"/water/{intake.Id}", intake);
});

// User Goals
app.MapGet("/goals/{userId}", async (int userId, AppDbContext db) =>
    await db.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId) is UserGoal goal 
        ? Results.Ok(goal) 
        : Results.NotFound());

app.MapPut("/goals/{userId}", async (int userId, UserGoal updatedGoal, AppDbContext db) =>
{
    var goal = await db.UserGoals.FirstOrDefaultAsync(g => g.UserId == userId);
    if (goal is null) return Results.NotFound();

    goal.TargetCalories = updatedGoal.TargetCalories;
    goal.TargetProtein = updatedGoal.TargetProtein;
    goal.TargetCarbs = updatedGoal.TargetCarbs;
    goal.TargetFat = updatedGoal.TargetFat;
    goal.TargetWater = updatedGoal.TargetWater;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDefaultEndpoints();

app.Run();

record ChatRequest(string Prompt);
public record UserRegistrationRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);



