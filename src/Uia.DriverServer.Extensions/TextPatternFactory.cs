/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Uia.DriverServer.Attributes;

using UIAutomationClient;

#pragma warning disable IDE0051 // Remove unused private members (used by reflection do not remove)
namespace Uia.DriverServer.Extensions
{
    /// <summary>
    /// Provides methods to retrieve text based on different UI Automation patterns.
    /// </summary>
    public static class TextPatternFactory
    {
        /// <summary>
        /// Retrieves text based on a given ID and pattern using reflection.
        /// </summary>
        /// <param name="id">The ID to match with the attribute.</param>
        /// <param name="pattern">The pattern object to be passed to the method.</param>
        /// <returns>The text retrieved by invoking the matched method, or an empty string if no match is found.</returns>
        [SuppressMessage(
            category: "Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
            Justification = "Reflection is used here to dynamically access methods with specific attributes that are not publicly accessible.")]
        public static string GetText(int id, object pattern)
        {
            // Local function to confirm if a method has the specified attribute and ID.
            static bool ConfirmAttribute(MethodInfo info, int id)
            {
                // Check if the method has the UiaConstantAttribute.
                var isAttribute = info.GetCustomAttribute<UiaConstantAttribute>() != null;

                // Check if the Constant property of the attribute matches the given ID.
                var isId = info.GetCustomAttribute<UiaConstantAttribute>().Constant == id;

                // Return true if both conditions are met, otherwise false.
                return isAttribute && isId;
            }

            // Binding flags to include non-public, static, and instance methods.
            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            // Get all methods of the TextPatternFactory class with the specified binding flags.
            var methods = typeof(TextPatternFactory).GetMethods(Flags);

            // Find the method that matches the attribute and ID.
            var method = Array.Find(methods, i => ConfirmAttribute(info: i, id));

            // If no method is found, return an empty string.
            if (method == null)
            {
                return string.Empty;
            }

            // Determine the instance to pass to Invoke. Use 'this' for instance methods and null for static methods.
            var instance = method.IsStatic ? null : typeof(TextPatternFactory);

            // Invoke the method and return its result as a string.
            return method.Invoke(instance, [pattern]).ToString();
        }

        // Gets the text using the TextChild pattern.
        [UiaConstant(UIA_PatternIds.UIA_TextChildPatternId)]
        private static string GetByTextChildPattern(object pattern)
        {
            // Cast the pattern object to IUIAutomationTextChildPattern.
            var textChildPattern = (IUIAutomationTextChildPattern)pattern;

            // Get the text from the text range and return it.
            return textChildPattern.TextRange.GetText(int.MaxValue);
        }

        // Gets the text using the TextEdit pattern.
        [UiaConstant(UIA_PatternIds.UIA_TextEditPatternId)]
        private static string GetByTextEditPattern(object pattern)
        {
            // Cast the pattern object to IUIAutomationTextEditPattern.
            var textEditPattern = (IUIAutomationTextEditPattern)pattern;

            // Get the text from the document range and return it.
            return textEditPattern.DocumentRange.GetText(-1);
        }

        // Gets the text using the TextPattern pattern.
        [UiaConstant(UIA_PatternIds.UIA_TextPatternId)]
        private static string GetByTextPattern(object pattern)
        {
            // Cast the pattern object to IUIAutomationTextPattern.
            var textPattern = (IUIAutomationTextPattern)pattern;

            // Get the text from the document range and return it.
            return textPattern.DocumentRange.GetText(-1);
        }

        // Gets the text using the TextPattern2 pattern.
        [UiaConstant(UIA_PatternIds.UIA_TextPattern2Id)]
        private static string GetByTextPattern2(object pattern)
        {
            // Cast the pattern object to IUIAutomationTextPattern2.
            var textPattern2 = (IUIAutomationTextPattern2)pattern;

            // Get the text from the document range and return it.
            return textPattern2.DocumentRange.GetText(-1);
        }

        // Gets the value using the ValuePattern pattern.
        [UiaConstant(UIA_PatternIds.UIA_ValuePatternId)]
        private static string GetByValuePattern(object pattern)
        {
            // Cast the pattern object to IUIAutomationValuePattern.
            var valuePattern = (IUIAutomationValuePattern)pattern;

            // Get the current value and return it.
            return valuePattern.CurrentValue;
        }
    }
}
