using System;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    public class ElementsRepository : IElementsRepository
    {
        public (int Status, UiaElementModel Element) FindElement(string session, LocationStrategyModel locationStrategy)
        {
            throw new NotImplementedException();
        }

        public (int Status, UiaElementModel Element) FindElement(string session, string element, LocationStrategyModel locationStrategy)
        {
            throw new NotImplementedException();
        }

        public UiaElementModel GetElement(string session, string element)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, string Value) GetElementAttribute(string session, string element, string attribute)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, string Text) GetElementText(string session, string element)
        {
            throw new NotImplementedException();
        }
    }
}
