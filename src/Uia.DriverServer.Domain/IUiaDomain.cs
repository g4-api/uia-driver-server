namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Interface for the UI Automation domain.
    /// </summary>
    public interface IUiaDomain
    {
        /// <summary>
        /// Gets the actions repository.
        /// </summary>
        IActionsRepository ActionsRepository { get; }

        /// <summary>
        /// Gets the document repository.
        /// </summary>
        IDocumentRepository DocumentRepository { get; }

        /// <summary>
        /// Gets the elements repository.
        /// </summary>
        IElementsRepository ElementsRepository { get; }

        #if Release_Emgu || Debug_Emgu
        /// <summary>
        /// Gets the OCR repository.
        /// </summary>
        IOcrRepository OcrRepository { get; }
        #endif

        /// <summary>
        /// Gets the sessions repository.
        /// </summary>
        ISessionsRepository SessionsRepository { get; }
    }
}
