using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Uia.DriverServer.Models;

using UIAutomationClient;

namespace Uia.DriverServer.Domain
{
    public class SessionRepository : ISessionRepository
    {
        public IDictionary<string, UiaSessionModel> Sessions => throw new NotImplementedException();

        public (int StatusCode, object Entity) CreateSession(UiaCapabilitiesModel capabilities)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, XDocument ElementsXml) NewDocumentObjectModel(string id)
        {
            throw new NotImplementedException();
        }

        public int DeleteSession(string id)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, string Entity) GetScreenshot()
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, UiaSessionModel Session) GetSession(string id)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, RectangleModel Entity) SetWindowVisualState(string id, WindowVisualState visualState)
        {
            throw new NotImplementedException();
        }
    }
}
