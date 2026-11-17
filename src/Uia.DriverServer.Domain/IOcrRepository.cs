using System.Collections.Generic;
using System.Drawing;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Represents a repository for handling OCR (Optical Character Recognition) operations.
    /// </summary>
    /// <remarks>
    /// Screen capture is always available. The recognition members exist only in the Emgu configurations, where
    /// the OCR engine is compiled into the driver.
    /// </remarks>
    public interface IOcrRepository
    {
        #region *** Methods      ***

#if RELEASE_EMGU || DEBUG_EMGU
        /// <summary>
        /// Captures the screen and finds the element addressed by an OCR locator.
        /// </summary>
        /// <param name="segment">The OCR locator, such as <c>ocr('Submit')[2]</c>.</param>
        /// <returns>The matching element, or <c>null</c> when no text on the screen satisfies the locator.</returns>
        UiaElementModel FindElement(string segment);

        /// <summary>
        /// Captures the screen and finds every element addressed by an OCR locator.
        /// </summary>
        /// <param name="segment">The OCR locator, such as <c>ocr('Submit')</c>.</param>
        /// <returns>The matching elements in reading order, or an empty list when nothing matches.</returns>
        List<UiaElementModel> FindElements(string segment);
#endif

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

#if RELEASE_EMGU || DEBUG_EMGU
        /// <summary>
        /// Captures the screen once and returns the image together with the words recognized on it.
        /// </summary>
        /// <remarks>
        /// The image and the words come from the same capture, which lets a client such as the OCR Inspector
        /// display exactly what the driver recognizes.
        /// </remarks>
        /// <returns>The screenshot as a Base64-encoded PNG image and its recognized words.</returns>
        OcrSnapshotModel NewSnapshot();

        /// <summary>
        /// Resolves the words on the specified bitmap using OCR.
        /// </summary>
        /// <param name="bitmap">The bitmap image to resolve text from; the caller keeps ownership.</param>
        /// <returns>The recognized words in reading order, with occurrence indexes and locators.</returns>
        List<OcrWordModel> Resolve(Bitmap bitmap);
#endif

        #endregion
    }
}
