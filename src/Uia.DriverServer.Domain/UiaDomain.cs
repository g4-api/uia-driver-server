using System;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    public class UiaDomain : IUiaDomain
    {
        public ISessionRepository SessionsRepository => throw new NotImplementedException();

        public IElementsRepository ElementsRepository => throw new NotImplementedException();

        public IDocumentRepository DocumentRepository => throw new NotImplementedException();

        public UiaSessionModel GetSession(string id)
        {
            throw new NotImplementedException();
        }
    }
}
