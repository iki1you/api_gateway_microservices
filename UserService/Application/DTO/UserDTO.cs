using Domain;

namespace Application.DTO
{
    public sealed class UserDTO(User user)
    {
        public long Id { get; set; } = user.Id;
        public string Login { get; set; } = user.Login;
        public string? FullName { get; set; } = user.FullName;
        public DateTime? BirthDay { get; set; } = user.BirthDay;
    }
}
