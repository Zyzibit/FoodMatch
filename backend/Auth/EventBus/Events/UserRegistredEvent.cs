using inzynierka.EventBus.Events;

namespace inzynierka.Auth.EventBus.Events
{
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

}
