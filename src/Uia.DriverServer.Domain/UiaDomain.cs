using System;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    public class UiaDomain : IUiaDomain
    {
        public UiaDomain(ISessionsRepository sessionRepository, IElementsRepository elementsRepository)
        {
            ElementsRepository = elementsRepository;
            SessionsRepository = sessionRepository;
        }

        public ISessionsRepository SessionsRepository { get; }

        public IElementsRepository ElementsRepository { get; }

        public IDocumentRepository DocumentRepository => throw new NotImplementedException();

        public UiaSessionModel GetSession(string id)
        {
            throw new NotImplementedException();
        }
    }
}
