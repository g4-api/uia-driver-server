/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Emgu.CV.OCR;

using System.Collections.Generic;
using System.Drawing;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Represents a repository for handling OCR (Optical Character Recognition) operations.
    /// </summary>
    public interface IOcrRepository
    {
        /// <summary>
        /// Finds an element using OCR with the specified segment.
        /// </summary>
        /// <param name="segment">The OCR segment used to locate the element.</param>
        /// <returns>A <see cref="UiaElementModel"/> representing the found element.</returns>
        UiaElementModel FindElement(string segment);

        /// <summary>
        /// Creates a new bitmap with default DPI (Dots Per Inch) settings.
        /// </summary>
        /// <returns>A <see cref="Bitmap"/> object representing the new bitmap.</returns>
        Bitmap NewBitmap();

        /// <summary>
        /// Creates a new bitmap with specified DPI settings.
        /// </summary>
        /// <param name="xDpi">The horizontal DPI.</param>
        /// <param name="yDpi">The vertical DPI.</param>
        /// <returns>A <see cref="Bitmap"/> object representing the new bitmap.</returns>
        Bitmap NewBitmap(float xDpi, float yDpi);

        /// <summary>
        /// Resolves text from the specified bitmap using OCR.
        /// </summary>
        /// <param name="bitmap">The bitmap image to resolve text from.</param>
        /// <returns>A list of <see cref="Tesseract.Word"/> objects representing the recognized words.</returns>
        List<Tesseract.Word> Resolve(Bitmap bitmap);
    }
}
