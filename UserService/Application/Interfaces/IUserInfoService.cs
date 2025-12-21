using Application.DTO;

namespace Application.Interfaces
{
    public interface IUserInfoService
    {
        public Task<UserDTO> GetUserInfo(long userId);
    }
}
