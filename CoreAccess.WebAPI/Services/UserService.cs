using CoreAccess.WebAPI.Repositories;

namespace CoreAccess.WebAPI.Services;

public interface IUserService
{
    
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
  //  private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
    }
}