using System;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    public class ElementsRepository : IElementsRepository
    {
        // use 'coords(100,200)' for coords based element
        // use 'ocr(a|b|c|d|f)' for ocr based element
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

        public (int StatusCode, string Value) GetElementAttribute(string session, string element, string name)
        {
            throw new NotImplementedException();
        }

        public (int StatusCode, string Text) GetElementText(string session, string element)
        {
            throw new NotImplementedException();
        }
    }
}
