namespace API.DTO.UserService
{
    public record UserDTO(long Id, string Login, string? FullName, string Email, DateTime? BirthDay);
}
