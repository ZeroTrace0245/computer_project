namespace computer_project.Web.Services;

using computer_project.Web.Models;

public class UserSession
{
    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public event Action? OnChange;

    public void Login(User user)
    {
        CurrentUser = user;
        NotifyStateChanged();
    }

    public void Logout()
    {
        CurrentUser = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
