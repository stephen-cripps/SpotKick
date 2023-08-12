using System.IO;
using System.Xml.Serialization;

namespace SpotKick.Application.UserRepository
{
    public class UserRepo : IUserRepo
    {
        private readonly XmlSerializer serialiser;
        private const string path = "./UserData.xml";

        public UserRepo()
        {
            serialiser = new XmlSerializer(typeof(UserData));
        }

        public void StoreCurrentUser(UserData data)
        {
            using var fileStream = new FileStream(path, FileMode.Create);
            serialiser.Serialize(fileStream, data);
        }

        public UserData GetPreviousUser()
        {
            if (!File.Exists(path))
                return null;

            using var stream = new FileStream(path, FileMode.Open);

            return serialiser.Deserialize(stream) as UserData;
        }

        public void ForgetUser()
        {
            File.Delete(path);
        }
    }
}
