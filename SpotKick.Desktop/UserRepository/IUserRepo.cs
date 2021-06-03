namespace SpotKick.Desktop.UserRepository
{
    public interface IUserRepo
    {
        void StoreCurrentUser(UserData data);

        UserData GetPreviousUser();

        void ForgetUser();
    }
}
