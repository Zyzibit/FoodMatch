using inzynierka.EventBus.Events;

namespace inzynierka.Auth.EventBus.Events
{
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
}
