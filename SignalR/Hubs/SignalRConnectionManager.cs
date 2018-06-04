using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SignalR.Hubs
{
    public class SignalRConnectionManager<T> : IDisposable
    {
        private readonly ConcurrentDictionary<T, HashSet<string>> _connections = new ConcurrentDictionary<T, HashSet<string>>();

        public int Count { get { return _connections.Count; } }

        /// <summary>
        /// Attempts to add the specified userid and connectionid
        /// </summary>
        public void Add(T userid, string connectionid)
        {
            HashSet<string> connections = _connections.GetOrAdd(userid, new HashSet<string>());

            lock (connections)
            {
                connections.Add(connectionid);
            }
        }

        public IEnumerable<string> Connections(T userid)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(userid, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<T> UserIds()
        {
            return _connections.Keys;
        }

        /// <summary>
        /// Attempts to remove a connectionid that has the specified userid
        /// </summary>
        public void Remove(T userid, string connectionid)
        {
            HashSet<string> connections;
            if (!_connections.TryGetValue(userid, out connections))
            {
                return;
            }

            lock (connections)
            {
                connections.Remove(connectionid);

                if (connections.Count == 0)
                {
                    HashSet<string> emptyConnections;
                    _connections.TryRemove(userid, out emptyConnections);
                }
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _connections.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}