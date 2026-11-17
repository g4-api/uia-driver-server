/*
 * CHANGE LOG - keep only last 5 threads
 *
 * RESOURCES
 *
 * NOTES
 * This file is mirrored verbatim by the OCR Inspector (https://github.com/g4-api/ocr-inspector) so both tools run
 * the same recognition pipeline on the same pixels. Apply every change to both repositories.
 *
 * The driver compiles this file only in the Emgu configurations (see Uia.DriverServer.Domain.csproj).
 */
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Runs the Tesseract recognition pipeline that turns a bitmap into the shared OCR word list.
    /// </summary>
    /// <remarks>
    /// The engine is stateless: every call owns and disposes its Tesseract instance and image buffers, so calls
    /// are safe from concurrent requests. Recognition settings are fixed here because any difference between the
    /// driver and the OCR Inspector changes the words and therefore the locators.
    /// </remarks>
    public static class OcrEngine
    {
        #region *** Constants    ***

        // Names the folder, next to the running executable, that holds the Tesseract trained data files.
        private const string TrainDataFolder = "TrainData";

        // Selects the trained language; both tools ship the same eng.traineddata file.
        private const string Language = "eng";

        #endregion

        #region *** Properties   ***

        /// <summary>
        /// Gets the trained data folder next to the running executable, ending with a directory separator.
        /// </summary>
        /// <remarks>
        /// The path is anchored to the application base directory, not the working directory, so recognition
        /// works no matter where the process was started from.
        /// </remarks>
        public static string DataPath
        {
            get
            {
                var folder = Path.Combine(AppContext.BaseDirectory, TrainDataFolder);

                return folder + Path.DirectorySeparatorChar;
            }
        }

        #endregion

        #region *** Methods      ***

        /// <summary>
        /// Recognizes the words on a bitmap using the trained data next to the running executable.
        /// </summary>
        /// <param name="bitmap">The image to recognize; the caller keeps ownership and the bitmap is not modified.</param>
        /// <returns>The recognized words, normalized, in reading order, with occurrence indexes and locators.</returns>
        public static List<OcrWordModel> ResolveWords(Bitmap bitmap)
        {
            // Route convenience callers through the complete overload with the default trained data folder.
            return ResolveWords(bitmap, dataPath: DataPath);
        }

        /// <summary>
        /// Recognizes the words on a bitmap using the trained data in the supplied folder.
        /// </summary>
        /// <remarks>
        /// The pipeline converts the bitmap to an OpenCV matrix, reduces it to grayscale, and runs Tesseract in
        /// sparse-text mode so text scattered across a user interface is found without assuming a page layout.
        /// Tesseract initialization failures, such as missing trained data, surface as the engine's exception.
        /// </remarks>
        /// <param name="bitmap">The image to recognize; the caller keeps ownership and the bitmap is not modified.</param>
        /// <param name="dataPath">The folder holding the trained data, ending with a directory separator.</param>
        /// <returns>The recognized words, normalized, in reading order, with occurrence indexes and locators.</returns>
        public static List<OcrWordModel> ResolveWords(Bitmap bitmap, string dataPath)
        {
            // Own the engine for this call only so concurrent requests never share native recognition state.
            using var ocr = new Tesseract(dataPath, language: Language, mode: OcrEngineMode.Default);
            ocr.PageSegMode = PageSegMode.SparseText;

            // Convert the bitmap into an OpenCV matrix and reduce it to the grayscale image Tesseract reads.
            using var image = bitmap.ToMat();
            using var grayImage = new Mat();
            var conversion = image.NumberOfChannels == 4 ? ColorConversion.Bgra2Gray : ColorConversion.Bgr2Gray;

            if (image.NumberOfChannels == 1)
            {
                image.CopyTo(grayImage);
            }
            else
            {
                CvInvoke.CvtColor(src: image, dst: grayImage, code: conversion);
            }

            // Recognize the text; the word rectangles are expressed in pixels of the supplied bitmap.
            ocr.SetImage(grayImage);
            ocr.Recognize();

            // Map the engine words onto the shared model before applying the shared ordering and indexing rules.
            var words = ocr
                .GetWords()
                .Select(word => new OcrWordModel
                {
                    Confidence = Math.Round(word.Confident, digits: 2),
                    Rectangle = new RectangleModel
                    {
                        Bottom = word.Region.Bottom,
                        Left = word.Region.Left,
                        Right = word.Region.Right,
                        Top = word.Region.Top
                    },
                    Text = word.Text
                });

            return OcrLocator.FormatWords(words);
        }

        #endregion
    }
}
