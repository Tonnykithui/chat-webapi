using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatSystem.Hubs
{
    public class ConnectionHub : Hub
    {
        private readonly string _botName;
        private readonly IDictionary<string, UserConnection> _connections;

        public ConnectionHub(IDictionary<string, UserConnection> connections)
        {
            _connections = connections;
            _botName = "234 BOT";
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if(_connections.TryGetValue(Context.ConnectionId, out UserConnection user))
            {
                _connections.Remove(Context.ConnectionId);

                Clients.Group(user.GroupName).SendAsync("ReceiveMess", _botName, $"{user.Name} has left group");

            }

            UsersInRoom(user.GroupName);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            if(_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.GroupName)
                    .SendAsync("ReceiveMess", userConnection.Name, message);
            }
        }


        public async Task JoinRoom(UserConnection user)
        {

            _connections[Context.ConnectionId] = user;
            await Groups.AddToGroupAsync(Context.ConnectionId, user.GroupName);

            await Clients.Group(user.GroupName).SendAsync("ReceiveMess", _botName, $"User {user.Name} added to the group");
            await UsersInRoom(user.GroupName);
        }

        public Task UsersInRoom(string room)
        {
            var users = _connections.Values
                .Where(c => c.GroupName == room)
                .Select(c => c.Name);

            return Clients.Group(room).SendAsync("UsersInRoom", users);

        }
    }
}
