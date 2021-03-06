using SharedHost.Models.Session;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Signalling.Interfaces
{
    public interface ISessionQueue
    {
        Task Handle(SessionAccession accession, WebSocket ws);
    }
}
