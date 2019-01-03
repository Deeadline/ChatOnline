using ChatOnline.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatOnline.Interface
{
    public interface ITypedHubClient
    {
        Task BroadcastMessage(Message message);
        Task ReceiveRoom(Room room);
        Task ReceiveRooms(List<Tuple<int, string>> roomNames);
    }
}
