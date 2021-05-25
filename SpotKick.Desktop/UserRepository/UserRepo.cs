using System.IO;
using System.Xml.Serialization;

namespace SpotKick.Desktop.UserRepository
{
    public class UserRepo : IUserRepo
    {
        readonly XmlSerializer serialiser;
        const string path = "./UserData.xml";

        public UserRepo()
        {
            serialiser = new XmlSerializer(typeof(UserData));
        }

        public void StoreCurrentUser(UserData data)
        {
            var file = File.Create(path);

            serialiser.Serialize(file, data);
            file.Close();
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
