// Several namespaces are consumed only by the members compiled in the Emgu configurations.
#pragma warning disable IDE0005
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Uia.DriverServer.Marshals;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Provides implementation for OCR (Optical Character Recognition) operations.
    /// </summary>
    /// <remarks>
    /// The repository owns screen capture and adapts OCR matches to driver elements. Recognition itself lives in
    /// <c>OcrEngine</c> and matching in <see cref="OcrLocator"/>, which the OCR Inspector mirrors, so the
    /// repository holds no recognition or matching rules of its own. It keeps no state between calls.
    /// </remarks>
    public class OcrRepository : IOcrRepository
    {
        #region *** Methods      ***

#if RELEASE_EMGU || DEBUG_EMGU
        /// <inheritdoc />
        public UiaElementModel FindElement(string segment)
        {
            // Resolve every match and expose the first; an occurrence suffix already narrows the list to one.
            return FindElements(segment).FirstOrDefault();
        }

        /// <inheritdoc />
        public List<UiaElementModel> FindElements(string segment)
        {
            // Capture and recognize the current screen; the bitmap is released as soon as the words exist.
            using var bitmap = NewBitmap();
            var words = OcrEngine.ResolveWords(bitmap);

            // Match the locator against the recognized words and adapt each match to a driver element.
            return [.. OcrLocator.FindWords(words, locator: segment).Select(NewElement)];
        }
#endif

        /// <inheritdoc />
        public Bitmap NewBitmap()
        {
            // Create a new bitmap with default DPI (300x300).
            return NewBitmap(xDpi: 300, yDpi: 300);
        }

        /// <inheritdoc />
        public Bitmap NewBitmap(float xDpi, float yDpi)
        {
            // Get the current display settings.
            var devMode = User32.GetDisplaySettings();

            // Create a bitmap with the dimensions of the primary display.
            var image = new Bitmap(
                width: devMode.dmPelsWidth,
                height: devMode.dmPelsHeight,
                format: PixelFormat.Format32bppArgb);

            // Capture the screen and copy it to the bitmap; the drawing surface is released after the copy.
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.CopyFromScreen(
                    sourceX: devMode.dmPositionX,
                    sourceY: devMode.dmPositionY,
                    destinationX: 0,
                    destinationY: 0,
                    blockRegionSize: image.Size);
            }

            // Set the resolution of the bitmap to the specified DPI.
            image.SetResolution(xDpi, yDpi);

            // Return the bitmap.
            return image;
        }

#if RELEASE_EMGU || DEBUG_EMGU
        /// <inheritdoc />
        public OcrSnapshotModel NewSnapshot()
        {
            // Capture once so the returned image and the recognized words describe the same pixels.
            using var bitmap = NewBitmap();
            var words = OcrEngine.ResolveWords(bitmap);

            // Encode the capture as PNG, the lossless format the screenshot endpoint also publishes.
            using var stream = new MemoryStream();
            bitmap.Save(stream, format: ImageFormat.Png);

            return new OcrSnapshotModel
            {
                Screenshot = Convert.ToBase64String(stream.ToArray()),
                Words = words
            };
        }

        /// <inheritdoc />
        public List<OcrWordModel> Resolve(Bitmap bitmap)
        {
            // Run the shared recognition pipeline; the caller keeps ownership of the bitmap.
            return OcrEngine.ResolveWords(bitmap);
        }

        // Adapts one OCR match to a driver element that later commands address through its rectangle.
        // The element has no UI Automation object; its XML node carries the text and attributes that the element
        // text and attribute endpoints read. The node is built through the XML API so any recognized character,
        // including markup characters, is escaped correctly.
        private static UiaElementModel NewElement(OcrWordModel word)
        {
            var id = $"{Guid.NewGuid()}";

            // Publish the match details as attributes; "Confident" keeps the name the driver has always used.
            var node = new XElement(
                "OcrElement",
                new XAttribute("Bottom", word.Rectangle.Bottom),
                new XAttribute("Confident", word.Confidence),
                new XAttribute("Left", word.Rectangle.Left),
                new XAttribute("Right", word.Rectangle.Right),
                new XAttribute("Top", word.Rectangle.Top),
                new XAttribute("Index", word.Index),
                new XAttribute("Locator", word.Locator),
                new XAttribute("Id", id),
                word.Text);

            return new UiaElementModel
            {
                Id = id,
                Node = new XDocument(node).Root,
                Rectangle = word.Rectangle
            };
        }
#endif

        #endregion
    }
}
