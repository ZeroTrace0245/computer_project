using System.Net.Http.Json;
using computer_project.Web.Models;

namespace computer_project.Web;

public class SmartBiteApiClient(HttpClient httpClient)
{
    public async Task<List<Meal>> GetMealsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Meal>>("/meals", cancellationToken) ?? [];
    }

    public async Task AddMealAsync(Meal meal, CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsJsonAsync("/meals", meal, cancellationToken);
    }

    public async Task<List<ShoppingListItem>> GetShoppingListAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<ShoppingListItem>>("/shoppinglist", cancellationToken) ?? [];
    }

    public async Task AddShoppingListItemAsync(ShoppingListItem item, CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsJsonAsync("/shoppinglist", item, cancellationToken);
    }

    public async Task UpdateShoppingListItemAsync(ShoppingListItem item, CancellationToken cancellationToken = default)
    {
        await httpClient.PutAsJsonAsync($"/shoppinglist/{item.Id}", item, cancellationToken);
    }

    public async Task<HealthReport?> GetReportAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<HealthReport>("/stats", cancellationToken);
    }

    public async Task<List<AIRecommendation>> GetRecommendationsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<AIRecommendation>>("/recommendations", cancellationToken) ?? [];
    }

    public async Task<List<WaterIntake>> GetWaterIntakesAsync(CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<List<WaterIntake>>("/water", ct) ?? [];
    }

    public async Task<string> GetWaterAdviceAsync(double current, double target, CancellationToken ct = default)
    {
        return await httpClient.GetStringAsync($"/water/advice?current={current}&target={target}", ct);
    }

    public async Task AddWaterIntakeAsync(WaterIntake intake, CancellationToken ct = default)
    {
        await httpClient.PostAsJsonAsync("/water", intake, ct);
    }

    public async Task<UserGoal?> GetGoalAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<UserGoal>($"/goals/{userId}", ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task UpdateGoalAsync(UserGoal goal, CancellationToken ct = default)
    {
        await httpClient.PutAsJsonAsync($"/goals/{goal.UserId}", goal, ct);
    }

    public async Task<string> GetAICoachAdviceAsync(string prompt, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/ai/chat", new { prompt }, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<bool> RegisterUserAsync(string username, string password, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/users", new { Username = username, Password = password }, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<User?> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/login", new { Username = username, Password = password }, ct);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<User>(ct);
        }
        return null;
    }

    public async Task<NutritionEstimation?> GetNutritionEstimationAsync(string mealDescription, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<NutritionEstimation>($"/ai/estimate?mealDescription={Uri.EscapeDataString(mealDescription)}", ct);
    }
}

public record NutritionEstimation(double Calories, double Protein, double Carbs, double Fat);


