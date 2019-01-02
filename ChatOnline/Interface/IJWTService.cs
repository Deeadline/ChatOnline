using ChatOnline.Models;

namespace ChatOnline.Interface
{
    public class JsonWebToken
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public long Expires { get; set; }
    }

    public interface IJWTService
    {
        string Generate(User data);
    }
}