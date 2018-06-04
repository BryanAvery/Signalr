using Microsoft.AspNet.SignalR;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    [Authorize]
    public class HeartBeatHub : Hub
    {
        private static Timer _timer;
        private readonly static SignalRConnectionManager<string> _connections = new SignalRConnectionManager<string>();
        private readonly object _HeartBeatLock = new object();

        public HeartBeatHub()
        { }

        public override Task OnConnected()
        {
            _connections.Add(Context.User.Identity.Name, Context.ConnectionId);
            Debug.WriteLine($"Added: {Context.User.Identity.Name}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _connections.Remove(Context.User.Identity.Name, Context.ConnectionId);
            Debug.WriteLine($"Removed: {Context.User.Identity.Name}");

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;
            if (!_connections.Connections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
                Debug.WriteLine($"Reconnected: {Context.User.Identity.Name}");
            }

            return base.OnReconnected();
        }

        public void Send(string name)
        {
            CheckHeartBeat(null);
            // This is simulating a data change, such as data being update in the database
            if (_timer == null)
            {
                _timer = new Timer(CheckHeartBeat, null, TimeSpan.FromMilliseconds(5000), TimeSpan.FromMilliseconds(5000));
            }
        }

        private void CheckHeartBeat(object state)
        {
            if (_connections.Count == 0)
            {
                return;
            }

            lock (_HeartBeatLock)
            {
                try
                {
                    var sb = new StringBuilder();
                    foreach (var userId in _connections.UserIds())
                    {
                        foreach (var connectionId in _connections.Connections(userId))
                        {
                            sb.Append($"[HeartbeatHub] user [{userId}] connection [{connectionId}]<br/>");
                        }
                    }

                    sb.Append($"tick-tock: {DateTime.UtcNow}<br/><br/>");
                    Clients.All.broadcastMessage(sb.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}