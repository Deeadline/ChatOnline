using ChatOnline.Database;
using ChatOnline.Interface;
using ChatOnline.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatOnline.Controllers
{
    public class ChatHub : Hub<ITypedHubClient>
    {
        private readonly ApplicationDbContext context;
        private static readonly Dictionary<string, List<string>> RoomConnections = new Dictionary<string, List<string>>();

        public ChatHub(ApplicationDbContext context) => this.context = context;

        public override Task OnConnectedAsync()
        {
            var rooms = context.Rooms.AsNoTracking().Select(x => Tuple.Create(x.Id, x.Name)).ToList();
            Clients.Client(Context.ConnectionId).ReceiveRooms(rooms).GetAwaiter().GetResult();
            return base.OnConnectedAsync();
        }

        // Użytkownik wchodzi do konkretnego pokoju
        public void EnterRoom(string roomName, int userId)
        {
            // Pobieramy użytkownika (wiemy, że istnieje), oraz pokój po jego nazwie
            var room = context.Rooms.Include(x => x.Messages).Include(y => y.Users).SingleOrDefault(room1 => room1.Name.Equals(roomName));
            var user = context.Users.Single(x => x.Id.Equals(userId));

            if (room != null)
            {
                // Uzupełniamy nasz pomocniczy słownik, ponieważ Context.Id nie jest tym samym co w bazie danych nasz user.
                if (!RoomConnections.ContainsKey(roomName))
                {
                    RoomConnections.Add(roomName, new List<string>());
                }

                var otherUsers = RoomConnections[roomName];
                RoomConnections[roomName].Add(Context.ConnectionId);

                // Tutaj sobie dodajemy do bazy danych do naszego pokoju użytkownika
                room.Users.Add(user);
                context.SaveChanges();
                if (otherUsers.Count > 0)
                {
                    Clients.Clients(otherUsers).ReceiveRoom(room).GetAwaiter().GetResult();
                }

                Clients.Client(Context.ConnectionId).ReceiveRoom(room).GetAwaiter().GetResult();
            }
        }

        // Kiedy uzytkownik zmieni pokoj A na B
        public void ChangeRoom(string roomName, int userId)
        {
            var name = string.Empty;
            //pobieramy pokoj z bazy danych po nazwie ze słownika
            foreach (var rooms in RoomConnections)
            {
                if (rooms.Value.Contains(Context.ConnectionId))
                {
                    name = rooms.Key;
                    rooms.Value.Remove(Context.ConnectionId);
                }
            }

            var oldRoomUsers = RoomConnections[name];

            //pobieramy naszego uzytkownika z bazy i pokoj + usuwamy z pokoju(db) uzytkownika
            var user = context.Users.Single(x => x.Id.Equals(userId));
            var oldRoom = context.Rooms.Include(m => m.Messages).Include(u => u.Users).SingleOrDefault(o => o.Name.Equals(name));

            if (oldRoom == null) return;

            oldRoom.Users.Remove(user);
            context.SaveChanges();

            Clients.Clients(oldRoomUsers).ReceiveRoom(oldRoom).GetAwaiter().GetResult();
            // i wchodzimy do pokoju
            EnterRoom(roomName, userId);
        }

        //Do wylogowania się kompletnie.
        public void LeaveRoom(int userId)
        {
            var user = context.Users.SingleOrDefault(x => x.Id.Equals(userId));
            var room = context.Rooms.Include(x => x.Messages).Include(y => y.Users).SingleOrDefault(r => r.Users.Contains(user));
            if (room == null) return;
            room.Users.Remove(user);
            context.SaveChanges();

            // i odswiezamy pokoj innym
            Clients.Clients(RoomConnections[room.Name]).ReceiveRoom(room).GetAwaiter().GetResult();
        }

        // Wysylamy wiadomosc do uzytkownikow + zapisujemy ja do obecnego pokoju.
        public void SendMessage(Message message)
        {
            var name = string.Empty;
            //pobieramy pokoj z bazy danych po nazwie ze słownika
            foreach (var rooms in RoomConnections)
            {
                if (rooms.Value.Contains(Context.ConnectionId))
                {
                    name = rooms.Key;
                }
            }
            var room = context.Rooms.SingleOrDefault(x => x.Name.Equals(name));
            if (room != null)
            {
                //dodajemy do bazy danych wiadomosc.
                context.Messages.Add(message);
                context.SaveChanges();
                room.Messages.Add(message);
                // pobieramy uzytkownikow z pokoju w ktorym jest nasz uzytkownik
                var users = new List<string>();
                foreach (var rooms in RoomConnections)
                {
                    if (rooms.Value.Contains(Context.ConnectionId))
                    {
                        users = rooms.Value;
                    }
                }
                context.SaveChanges();
                // i wysylamy im wszystkim nowa wiadomosc
                Clients.Clients(users).BroadcastMessage(message).GetAwaiter().GetResult();
            }
        }
    }
}
