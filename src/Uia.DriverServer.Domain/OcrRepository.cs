using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Uia.DriverServer.Marshals;

namespace Uia.DriverServer.Domain
{
    public class OcrRepository
    {
        public void GetScreenShot()
        {
            throw new NotImplementedException();
        }

        public void FindElement(string segment)
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

            var a = "";
        }

        // Processes the image at the given path using Tesseract OCR, converts it to grayscale, recognizes the text,
        // draws bounding boxes around the recognized words, and returns the processed image along with the list of recognized words.
        private static (object ImageSource, List<Tesseract.Word> Words) ResolveWords(string imagePath)
        {
            // Load the image
            using var image = new Image<Bgr, byte>(imagePath);

            // Initialize Tesseract OCR
            using var ocr = new Tesseract("TrainData/", "eng", OcrEngineMode.Default);
            ocr.PageSegMode = Emgu.CV.OCR.PageSegMode.SparseText;
            // Convert to grayscale for OCR processing (Tesseract requires a grayscale image)
            using var grayImage = image.Convert<Gray, byte>();

            // Perform OCR on the grayscale image and get the recognized words
            ocr.SetImage(grayImage);
            ocr.Recognize();

            // Get the words from the OCR result
            var words = ocr.GetWords().ToList();

            // Save or display the image
            return (default, words);
        }

        // Converts the provided Bitmap for processing with Tesseract OCR, converts it to grayscale, recognizes the text,
        // draws bounding boxes around the recognized words, and returns the processed image along with the list of recognized words.
        private static (object ImageSource, List<Tesseract.Word> Words) ResolveWords(Bitmap image)
        {
            // Load the image
            using var processedImage = image.ToImage<Bgr, byte>();

            // Initialize Tesseract OCR
            using var ocr = new Tesseract("TrainData/", "eng", OcrEngineMode.Default);
            ocr.PageSegMode = PageSegMode.SparseText;

            // Convert to grayscale for OCR processing (Tesseract requires a grayscale image)
            using var grayImage = processedImage.Convert<Gray, byte>();

            // Perform OCR on the grayscale image and get the recognized words
            ocr.SetImage(grayImage);
            ocr.Recognize();

            // Get the words from the OCR result
            var words = ocr.GetWords().ToList();

            // Save or display the image
            return (default, words);
        }
    }
}
