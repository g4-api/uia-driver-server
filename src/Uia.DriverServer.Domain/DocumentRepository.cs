using System;
using System.Threading.Tasks;

namespace Uia.DriverServer.Domain
{
    public class DocumentRepository : IDocumentRepository
    {
        public (int StatusCode, string Result) GetPageSource(string session)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, object Result) InvokeScript(string session, string src)
        {
            throw new NotImplementedException();
        }

        public Task<(int StatusCode, object Result)> InvokeScriptAsync(string session, string src)
        {
            throw new NotImplementedException();
        }
    }
}
