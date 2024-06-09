using System;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    public class UiaDomain : IUiaDomain
    {
        public UiaDomain(
            IElementsRepository elementsRepository,
            IOcrRepository ocrRepository,
            ISessionsRepository sessionRepository)
        {
            ElementsRepository = elementsRepository;
            OcrRepository = ocrRepository;
            SessionsRepository = sessionRepository;
        }

        public IDocumentRepository DocumentRepository => throw new NotImplementedException();

        public IElementsRepository ElementsRepository { get; }

        public IOcrRepository OcrRepository { get; }
        public ISessionsRepository SessionsRepository { get; }

        

        

        public UiaSessionModel GetSession(string id)
        {
            throw new NotImplementedException();
        }
    }
}
