using System.Threading.Tasks;
using SpotKick.Application.UserRepository;

namespace SpotKick.Application.Services;

public interface ICurrentUserService
{
    Task<UserData> ValidateAndGetCurrentUserAsync();
    
    UserData GetCurrentUser();

    void StoreSongkickUsername(string username);

    void ForgetCurrentUser();
}