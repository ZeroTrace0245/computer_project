using computer_project.ApiService.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace computer_project.ApiService.Services;

public class AIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var section = configuration.GetSection("GoogleAI");
        _apiKey = section["ApiKey"] ?? throw new InvalidOperationException("GoogleAI:ApiKey is missing");
        _model = section["Model"] ?? "gemini-1.5-flash";
    }

    public async Task<string> GetChatResponseAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}",
                requestBody);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            return result?.Candidates?[0].Content.Parts[0].Text.Trim() ?? "No response from AI.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google AI");
            return $"AI is currently unavailable. Error: {ex.Message}";
        }
    }

    public async Task<MealNutritionStub?> EstimateNutritionAsync(string mealDescription)
    {
        try
        {
            var prompt = $"Analyze this meal: '{mealDescription}'. " +
                         "Estimate its nutritional values. Return ONLY a JSON object with properties: " +
                         "Calories (double), Protein (double), Carbs (double), Fat (double). " +
                         "Ensure its valid JSON.";

            var content = await GetChatResponseAsync(prompt);
            content = CleanJsonResponse(content);

            return JsonSerializer.Deserialize<MealNutritionStub>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating nutrition for {Meal}", mealDescription);
            return null;
        }
    }

    public record MealNutritionStub(double Calories, double Protein, double Carbs, double Fat);

    public async Task<List<AIRecommendation>> GetRecommendationsAsync(List<Meal> recentMeals, UserGoal? goal)
    {
        try
        {
            var mealSummary = string.Join(", ", recentMeals.Select(m => $"{m.Name} ({m.Calories} kcal)"));
            var goalSummary = goal != null 
                ? $"Goal: {goal.TargetCalories} kcal, P: {goal.TargetProtein}g, C: {goal.TargetCarbs}g, F: {goal.TargetFat}g"
                : "No specific goal set.";

            var prompt = $"Based on these recent meals: {mealSummary}. {goalSummary}. " +
                         "Suggest 2 healthy meals. Return ONLY a JSON array of objects with properties: " +
                         "SuggestedMeal (string), Reason (string), EstimatedCalories (int).";

            var content = await GetChatResponseAsync(prompt);
            content = CleanJsonResponse(content);

            return JsonSerializer.Deserialize<List<AIRecommendation>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI recommendations");
            return [];
        }
    }

    public async Task<string> GenerateHealthReportAsync(HealthReport report, UserGoal? goal)
    {
        try
        {
            var prompt = $"Analyze this health report: Total Calories: {report.TotalCalories}, " +
                         $"Protein: {report.Protein}g, Carbs: {report.Carbs}g, Fat: {report.Fat}g. " +
                         $"Target Calories: {goal?.TargetCalories ?? 2000}. " +
                         "Provide a concise, encouraging 2-3 sentence health assessment.";

            return await GetChatResponseAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating health report assessment");
            return "Could not generate assessment at this time.";
        }
    }

    public async Task<string> GetWaterAdviceAsync(double totalWater, double targetWater)
    {
        try
        {
            var prompt = $"The user has drunk {totalWater:F1}L of water today. " +
                         $"Their goal is {targetWater:F1}L. " +
                         "Provide a short, motivating tip (max 20 words) for their water intake.";

            return await GetChatResponseAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting water advice");
            return "Stay hydrated! Remember to drink water throughout the day.";
        }
    }

    private string CleanJsonResponse(string content)
    {
        content = content.Trim();
        if (content.StartsWith("```json")) content = content[7..].Trim();
        else if (content.StartsWith("```")) content = content[3..].Trim();
        if (content.EndsWith("```")) content = content[..^3].Trim();
        return content;
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }

        public class Candidate
        {
            [JsonPropertyName("content")]
            public Content Content { get; set; } = null!;
        }

        public class Content
        {
            [JsonPropertyName("parts")]
            public Part[] Parts { get; set; } = null!;
        }

        public class Part
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = null!;
        }
    }
}
