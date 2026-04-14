namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель пользователя системы
    /// </summary>
    public class UserModel
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }      // "Администратор" или "Диспетчер"
        public bool IsActive { get; set; }
    }
}
