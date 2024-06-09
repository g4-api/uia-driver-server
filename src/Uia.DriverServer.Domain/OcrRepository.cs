/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

using Uia.DriverServer.Marshals;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Provides implementation for OCR (Optical Character Recognition) operations.
    /// </summary>
    public class OcrRepository : IOcrRepository
    {
        /// <inheritdoc />
        public UiaElementModel FindElement(string segment)
        {
            // Get the current display settings.
            var devMode = User32.GetDisplaySettings();

            // Create a bitmap with the dimensions of the primary display.
            var image = new Bitmap(
                width: devMode.dmPelsWidth,
                height: devMode.dmPelsHeight,
                format: System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Capture the screen and copy it to the bitmap.
            Graphics.FromImage(image).CopyFromScreen(
                sourceX: devMode.dmPositionX,
                sourceY: devMode.dmPositionY,
                destinationX: 0,
                destinationY: 0,
                blockRegionSize: image.Size);

            // Set the resolution of the bitmap to 300 DPI.
            image.SetResolution(300, 300);

            // Process the image using Tesseract OCR to recognize words and draw bounding boxes around them.
            var resolvedImage = ResolveWords(image);

            // Split the segment string by the '|' character to get individual segments.
            var segments = segment.Split('|');

            // Find the word that matches any of the segments.
            var word = resolvedImage.Find(i => segments.Contains(i.Text));

            // Initialize a new rectangle model with the region of the found word.
            var rectangle = new RectangleModel
            {
                Bottom = word.Region.Bottom,
                Left = word.Region.Left,
                Right = word.Region.Right,
                Top = word.Region.Top
            };

            // Generate a new ID for the UIA element.
            var id = $"{Guid.NewGuid()}";

            // Create an XML representation of the OCR element with the word and rectangle information.
            var xml = "<OcrElement " +
                $"Bottom=\"{rectangle.Bottom}\" " +
                $"Confident=\"{word.Confident}\" " +
                $"Left=\"{rectangle.Left}\" " +
                $"Right=\"{rectangle.Right}\" " +
                $"Top=\"{rectangle.Top}\" " +
                $"Id=\"{id}\">{word.Text}</OcrElement>";

            // Return a new UIA element model with the rectangle and a generated ID.
            return new UiaElementModel
            {
                Id = id,
                Node = XDocument.Parse(xml).Root,
                Rectangle = rectangle
            };
        }

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
                format: System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Capture the screen and copy it to the bitmap.
            Graphics.FromImage(image).CopyFromScreen(
                sourceX: devMode.dmPositionX,
                sourceY: devMode.dmPositionY,
                destinationX: 0,
                destinationY: 0,
                blockRegionSize: image.Size);

            // Set the resolution of the bitmap to the specified DPI.
            image.SetResolution(xDpi, yDpi);

            // Return the bitmap.
            return image;
        }

        /// <inheritdoc />
        public List<Tesseract.Word> Resolve(Bitmap bitmap)
        {
            // Process the bitmap using Tesseract OCR and return the list of recognized words.
            return ResolveWords(bitmap);
        }

        // Converts the provided bitmap for processing with Tesseract OCR, converts it to grayscale, recognizes the text,
        // draws bounding boxes around the recognized words, and returns the list of recognized words.
        private static List<Tesseract.Word> ResolveWords(Bitmap bitmap)
        {
            // Load the image into an Emgu CV image object.
            using var processedImage = bitmap.ToImage<Bgr, byte>();

            // Initialize Tesseract OCR with the specified language and engine mode.
            using var ocr = new Tesseract("TrainData/", "eng", OcrEngineMode.Default);
            ocr.PageSegMode = PageSegMode.SparseText;

            // Convert to grayscale for OCR processing (Tesseract requires a grayscale image).
            using var grayImage = processedImage.Convert<Gray, byte>();

            // Perform OCR on the grayscale image and get the recognized words.
            ocr.SetImage(grayImage);
            ocr.Recognize();

            // Get the words from the OCR result and return them.
            return [.. ocr.GetWords()];
        }
    }
}
