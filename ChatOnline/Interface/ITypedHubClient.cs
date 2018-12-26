using ChatOnline.Models;
using System.Threading.Tasks;

namespace ChatOnline.Interface
{
    public interface ITypedHubClient
    {
        Task BroadcastMessage(Message message);
    }
}
