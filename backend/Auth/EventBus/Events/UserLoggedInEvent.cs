using inzynierka.EventBus.Events;

namespace inzynierka.Auth.EventBus.Events
{
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
}
