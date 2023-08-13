using System.Threading.Tasks;
using SpotKick.Application.SpotifyAuth;
using SpotKick.Application.UserRepository;

namespace SpotKick.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IUserRepo userRepo;
    private readonly ISpotifyAuthService spotifyAuthService;
    private UserData currentUser;

    public CurrentUserService(IUserRepo userRepo, ISpotifyAuthService spotifyAuthService)
    {
        this.userRepo = userRepo;
        this.spotifyAuthService = spotifyAuthService;
        currentUser = userRepo.GetPreviousUser() ?? new UserData();
    }

    public async Task<UserData> ValidateAndGetCurrentUserAsync()
    {
        if (currentUser.SpotifyCredentials.AccessToken == null)
        {
            currentUser.SpotifyCredentials = await spotifyAuthService.LogIn();
            currentUser.SpotifyUser =
                await spotifyAuthService.GetCurrentUser(currentUser.SpotifyCredentials.AccessToken);
            userRepo.StoreCurrentUser(currentUser);
        }

        if (!currentUser.SpotifyCredentials.UserIsValid)
        {
            await spotifyAuthService.RefreshAccessToken(currentUser.SpotifyCredentials.RefreshToken);
            userRepo.StoreCurrentUser(currentUser);
        }


        return currentUser;
    }

    public UserData GetCurrentUser()
    {
        return currentUser;
    }

    public void StoreSongkickUsername(string username)
    {
        currentUser.SongKickUsername = username;
        userRepo.StoreCurrentUser(currentUser);
    }

    public void ForgetCurrentUser()
    {
        currentUser = null;
        userRepo.ForgetUser();
    }
}