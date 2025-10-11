using inzynierka.EventBus.Events;

namespace inzynierka.Auth.EventBus.Events
{
    public class TokenRefreshedEvent : BaseIntegrationEvent
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime RefreshTime { get; set; }

        public TokenRefreshedEvent()
        {
            ModuleName = "Auth";
        }
    }
}
