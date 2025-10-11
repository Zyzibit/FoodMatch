using inzynierka.EventBus.Events;

namespace inzynierka.Auth.EventBus.Events
{
    public class TokenValidatedEvent : BaseIntegrationEvent
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsValid { get; set; }

        public TokenValidatedEvent()
        {
            ModuleName = "Auth";
        }
    }
}
