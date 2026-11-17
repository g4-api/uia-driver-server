/*
 * CHANGE LOG - keep only last 5 threads
 *
 * RESOURCES
 *
 * NOTES
 * This file is mirrored verbatim by the OCR Inspector (https://github.com/g4-api/ocr-inspector) so both tools
 * exchange and display the same OCR contract. Apply every change to both repositories.
 */
using System.Collections.Generic;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents one screen capture together with every OCR word the driver resolved from it.
    /// </summary>
    /// <remarks>
    /// The snapshot is the mirror contract between the driver and the OCR Inspector: the image and the words come
    /// from the same capture, so a locator picked from the snapshot resolves to the same word inside the driver.
    /// </remarks>
    public class OcrSnapshotModel
    {
        /// <summary>
        /// Gets or sets the captured screen as a Base64-encoded PNG image.
        /// </summary>
        public string Screenshot { get; set; }

        /// <summary>
        /// Gets or sets the OCR words resolved from the screenshot, in reading order.
        /// </summary>
        public List<OcrWordModel> Words { get; set; } = [];
    }

    /// <summary>
    /// Represents one OCR word, or one matched phrase, located on a captured image.
    /// </summary>
    /// <remarks>
    /// The model carries only the values needed to display, address, and click the text; the OCR engine's
    /// internal layout data is intentionally not modeled.
    /// </remarks>
    public class OcrWordModel
    {
        /// <summary>
        /// Gets or sets the OCR engine confidence for the text, from 0 to 100.
        /// </summary>
        /// <remarks>A matched phrase reports the lowest confidence among its words.</remarks>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the 1-based occurrence of the text among identical texts, in reading order.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the ready-to-use OCR locator that resolves to this text.
        /// </summary>
        /// <remarks>The locator carries an occurrence index only when the same text appears more than once.</remarks>
        public string Locator { get; set; }

        /// <summary>
        /// Gets or sets the bounding rectangle of the text, in pixels of the captured image.
        /// </summary>
        public RectangleModel Rectangle { get; set; }

        /// <summary>
        /// Gets or sets the recognized text with surrounding whitespace removed.
        /// </summary>
        public string Text { get; set; }
    }
}
