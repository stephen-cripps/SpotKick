using System.Threading.Tasks;
using SpotKick.Application.Exceptions;
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

    public async Task<UserData> ValidateAndGetCurrentUserAsync(bool getUsername = true)
    {
        if (currentUser.SpotifyCredentials.AccessToken == null)
            await Login(getUsername);

        if (!currentUser.SpotifyCredentials.ShouldRefresh)
            await Refresh();

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

    private async Task Login(bool getUsername)
    {
        currentUser.SpotifyCredentials = await spotifyAuthService.LogIn();
        if (getUsername)
            currentUser.SpotifyUser =
                await spotifyAuthService.GetCurrentUser(currentUser.SpotifyCredentials.AccessToken);

        userRepo.StoreCurrentUser(currentUser);
    }

    private async Task Refresh()
    {
        try
        {
            currentUser.SpotifyCredentials.RefreshToken =
                (await spotifyAuthService.RefreshAccessToken(currentUser.SpotifyCredentials.RefreshToken)).RefreshToken;
        }
        catch (SpotifyAuthException e)
        {
            await Login(false);
        }

        userRepo.StoreCurrentUser(currentUser);
    }
}