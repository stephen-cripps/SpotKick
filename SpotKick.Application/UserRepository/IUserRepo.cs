namespace SpotKick.Application.UserRepository
{
    public interface IUserRepo
    {
        void StoreCurrentUser(UserData data);

        UserData GetPreviousUser();

        void ForgetUser();
    }
}
