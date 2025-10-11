using inzynierka.EventBus.Events;

namespace inzynierka.Auth.EventBus.Events
{
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
}
