using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    public interface IUiaDomain
    {
        ISessionsRepository SessionsRepository { get; }

        IElementsRepository ElementsRepository { get; }

        IDocumentRepository DocumentRepository { get; }

        public UiaSessionModel GetSession(string id);
    }
}
