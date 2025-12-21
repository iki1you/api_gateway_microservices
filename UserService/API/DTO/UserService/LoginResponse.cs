namespace API.DTO.UserService
{
    public record LoginResponse(string Token, UserDTO User);
}
