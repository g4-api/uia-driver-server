/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Defines methods for element operations in a Windows desktop automation session.
    /// </summary>
    public interface IElementsRepository
    {
        /// <summary>
        /// Finds an element based on the specified location strategy within a session.
        /// </summary>
        /// <param name="session">The unique identifier for the automation session.</param>
        /// <param name="locationStrategy">The strategy to locate the element.</param>
        /// <returns>
        /// A tuple containing:
        /// - Status: An HTTP status code indicating the result of the operation.
        /// - Element: The found element model.
        /// </returns>
        (int Status, UiaElementModel ElementModel) FindElement(string session, LocationStrategyModel locationStrategy);

        /// <summary>
        /// Finds an element based on the specified session, element identifier, and location strategy.
        /// </summary>
        /// <param name="session">The unique identifier for the automation session.</param>
        /// <param name="element">The identifier of the element to find.</param>
        /// <param name="locationStrategy">The strategy to locate the element.</param>
        /// <returns>
        /// A tuple containing:
        /// - Status: An HTTP status code indicating the result of the operation.
        /// - Element: The found element model.
        /// </returns>
        (int Status, UiaElementModel ElementModel) FindElement(string session, string element, LocationStrategyModel locationStrategy);

        /// <summary>
        /// Gets the element model based on the specified session and element identifier.
        /// </summary>
        /// <param name="session">The unique identifier for the automation session.</param>
        /// <param name="element">The identifier of the element to retrieve.</param>
        /// <returns>The element model.</returns>
        UiaElementModel GetElement(string session, string element);

        /// <summary>
        /// Gets the value of the specified attribute of an element within a session.
        /// </summary>
        /// <param name="session">The unique identifier for the automation session.</param>
        /// <param name="element">The identifier of the element to get the attribute from.</param>
        /// <param name="name">The name of the attribute to retrieve.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Value: The value of the specified attribute.
        /// </returns>
        (int StatusCode, string Value) GetElementAttribute(string session, string element, string name);

        /// <summary>
        /// Gets the text content of the specified element within a session.
        /// </summary>
        /// <param name="session">The unique identifier for the automation session.</param>
        /// <param name="element">The identifier of the element to get text from.</param>
        /// <returns>
        /// A tuple containing:
        /// - StatusCode: An HTTP status code indicating the result of the operation.
        /// - Text: The text content of the element.
        /// </returns>
        (int StatusCode, string Text) GetElementText(string session, string element);
    }
}
