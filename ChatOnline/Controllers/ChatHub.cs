using ChatOnline.Interface;
using Microsoft.AspNetCore.SignalR;

namespace ChatOnline.Controllers
{
    public class ChatHub : Hub<ITypedHubClient>
    {

    }
}
