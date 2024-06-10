/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Interface for the UI Automation domain.
    /// </summary>
    public interface IUiaDomain
    {
        /// <summary>
        /// Gets the document repository.
        /// </summary>
        IDocumentRepository DocumentRepository { get; }

        /// <summary>
        /// Gets the elements repository.
        /// </summary>
        IElementsRepository ElementsRepository { get; }

        /// <summary>
        /// Gets the OCR repository.
        /// </summary>
        IOcrRepository OcrRepository { get; }

        /// <summary>
        /// Gets the sessions repository.
        /// </summary>
        ISessionsRepository SessionsRepository { get; }
    }
}
