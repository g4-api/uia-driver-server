/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Implementation of the <see cref="IUiaDomain"/> interface.
    /// </summary>
    /// <param name="actionsRepository">The actions repository.</param>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="elementsRepository">The elements repository.</param>
    /// <param name="ocrRepository">The OCR repository.</param>
    /// <param name="sessionRepository">The sessions repository.</param>
    public class UiaDomain(
        IActionsRepository actionsRepository,
        IDocumentRepository documentRepository,
        IElementsRepository elementsRepository,
        IOcrRepository ocrRepository,
        ISessionsRepository sessionRepository) : IUiaDomain
    {
        /// <inheritdoc />
        public IActionsRepository ActionsRepository { get; } = actionsRepository;

        /// <inheritdoc />
        public IDocumentRepository DocumentRepository { get; } = documentRepository;

        /// <inheritdoc />
        public IElementsRepository ElementsRepository { get; } = elementsRepository;

        /// <inheritdoc />
        public IOcrRepository OcrRepository { get; } = ocrRepository;

        /// <inheritdoc />
        public ISessionsRepository SessionsRepository { get; } = sessionRepository;
    }
}
