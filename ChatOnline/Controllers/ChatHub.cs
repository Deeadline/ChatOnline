using ChatOnline.Database;
using ChatOnline.Interface;
using ChatOnline.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatOnline.Controllers
{
    public class ChatHub : Hub<ITypedHubClient>
    {
        private readonly ApplicationDbContext context;
        private readonly Dictionary<string, List<string>> roomConnections = new Dictionary<string, List<string>>();


        public ChatHub(ApplicationDbContext context) => this.context = context;

        public override Task OnConnectedAsync() => base.OnConnectedAsync();

        // Użytkownik wchodzi do konkretnego pokoju
        public void EnterRoom(string roomName, int userId)
        {
            // Pobieramy użytkownika (wiemy, że istnieje), oraz pokój po jego nazwie
            var room = context.Rooms.SingleOrDefault(room1 => room1.Name.Equals(roomName));
            var user = context.Users.Single(x => x.Id.Equals(userId));

            if (room != null)
            {
                // Uzupełniamy nasz pomocniczy słownik, ponieważ Context.Id nie jest tym samym co w bazie danych nasz user.
                if (!roomConnections.ContainsKey(roomName))
                {
                    roomConnections.Add(roomName, new List<string>());
                }
                roomConnections[roomName].Add(Context.ConnectionId);

                // Tutaj sobie dodajemy do bazy danych do naszego pokoju użytkownika
                room.Users.Add(user);
                context.SaveChanges();
                Clients.Client(Context.ConnectionId).Reload(room).GetAwaiter().GetResult();
            }
        }

        // Kiedy uzytkownik zmieni pokoj A na B
        public void ChangeRoom(string roomName, int userId)
        {
            //pobieramy naszego uzytkownika z bazy i pokoj + usuwamy z pokoju(db) uzytkownika
            var user = context.Users.Single(x => x.Id.Equals(userId));
            var oldRoom = context.Rooms.Single(r => r.Users.Contains(user));
            oldRoom.Users.Remove(user);
            context.SaveChanges();

            // a tutaj z naszego słownika usuwamy Context.ConnectionId
            foreach (var rooms in roomConnections)
            {
                if (rooms.Value.Contains(Context.ConnectionId))
                {
                    rooms.Value.Remove(Context.ConnectionId);
                }
            }
            // i wchodzimy do pokoju
            EnterRoom(roomName, userId);
        }

        //Do wylogowania się kompletnie.
        public void LeaveRoom(int userId)
        {
            var user = context.Users.Single(x => x.Id.Equals(userId));
            var room = context.Rooms.Single(r => r.Users.Contains(user));
            room.Users.Remove(user);
            context.SaveChanges();

            // tutaj z naszego słownika usuwamy Context.ConnectionId i pobieramy liste dostepnych uzytkownikow w pokoju
            foreach (var rooms in roomConnections)
            {
                if (rooms.Value.Contains(Context.ConnectionId))
                {
                    rooms.Value.Remove(Context.ConnectionId);
                }
            }
            var users = new List<string>();
            foreach (var rooms in roomConnections)
            {
                if (rooms.Value.Contains(Context.ConnectionId))
                {
                    users = rooms.Value;
                }
            }

            context.Update(room);
            context.SaveChanges();
            // i odswiezamy pokoj innym
            Clients.Clients(users).Reload(room).GetAwaiter().GetResult();

        }

        // Wysylamy wiadomosc do uzytkownikow + zapisujemy ja do obecnego pokoju.
        public void SendMessage(Message message)
        {
            var name = string.Empty;
            //pobieramy pokoj z bazy danych po nazwie ze słownika
            foreach (var rooms in roomConnections)
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
                foreach (var rooms in roomConnections)
                {
                    if (rooms.Value.Contains(Context.ConnectionId))
                    {
                        users = rooms.Value;
                    }
                }

                context.Update(room);
                context.SaveChanges();
                // i wysylamy im wszystkim nowa wiadomosc
                Clients.Clients(users).BroadcastMessage(message).GetAwaiter().GetResult();
            }
        }
    }
}
