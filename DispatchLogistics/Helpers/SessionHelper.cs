using DispatchLogistics.Models;

namespace DispatchLogistics.Helpers
{
    /// <summary>
    /// Глобальный хранитель информации о текущем сеансе пользователя
    /// </summary>
    public static class SessionHelper
    {
        public static UserModel CurrentUser { get; set; }
    }
}
