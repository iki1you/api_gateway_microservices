using Application.DTO;
using Application.Interfaces;
using Infrastructure.Interfaces;

namespace Application.Services
{
    public class UserInfoService(IUserRepository userRepository) : IUserInfoService
    {
        private IUserRepository _userRepository = userRepository;

        public async Task<UserDTO> GetUserInfo(long userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            return new UserDTO(user);
        }
    }
}
