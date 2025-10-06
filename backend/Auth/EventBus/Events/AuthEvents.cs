using inzynierka.EventBus.Events;

namespace inzynierka.Auth.EventBus.Events;

/// <summary>
/// Zdarzenia zwi¹zane z autoryzacj¹
/// </summary>
public class UserRegisteredEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public UserRegisteredEvent()
    {
        ModuleName = "Auth";
    }
}

public class UserLoggedInEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }

    public UserLoggedInEvent()
    {
        ModuleName = "Auth";
    }
}

public class TokenValidatedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public bool IsValid { get; set; }

    public TokenValidatedEvent()
    {
        ModuleName = "Auth";
    }
}

public class UserLoggedOutEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime LogoutTime { get; set; }

    public UserLoggedOutEvent()
    {
        ModuleName = "Auth";
    }
}

public class TokenRefreshedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public DateTime RefreshTime { get; set; }

    public TokenRefreshedEvent()
    {
        ModuleName = "Auth";
    }
}

public class TokenRevokedEvent : BaseIntegrationEvent
{
    public string UserId { get; set; } = string.Empty;
    public string TokenId { get; set; } = string.Empty;
    public DateTime RevokeTime { get; set; }

    public TokenRevokedEvent()
    {
        ModuleName = "Auth";
    }
}