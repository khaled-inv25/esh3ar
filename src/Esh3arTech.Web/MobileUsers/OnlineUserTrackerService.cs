using Esh3arTech.MobileUsers;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Web.MobileUsers
{
    public class OnlineUserTrackerService : ISingletonDependency
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _onlineUsers = new();

        public void AddConnection(string mobileNumber, string connectionId)
        {
            var connections = _onlineUsers.GetOrAdd(mobileNumber, _ => new ConcurrentBag<string>());
            connections.Add(connectionId);
        }

        public void RemoveConnection(string mobileNumber, string connectionId)
        {
            if (_onlineUsers.TryGetValue(mobileNumber, out var connections))
            {
                var newConnections = new ConcurrentBag<string>(connections.Where(id => id != connectionId));

                if (newConnections.IsEmpty)
                {
                    _onlineUsers.TryRemove(mobileNumber, out _);
                }
                else
                {
                    _onlineUsers.TryUpdate(mobileNumber, newConnections, connections);
                }
            }
        }

        public async Task<bool> IsConnectedAsync(string mobileNumber)
        {
            return _onlineUsers.TryGetValue(mobileNumber, out var connections) && connections.Any();
        }
    }
}
