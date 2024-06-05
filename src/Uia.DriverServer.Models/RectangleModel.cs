/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents a rectangle with properties for the top, bottom, left, and right edges.
    /// </summary>
    public class RectangleModel
    {
        /// <summary>
        /// Gets or sets the bottom edge of the rectangle.
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// Gets or sets the left edge of the rectangle.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Gets or sets the right edge of the rectangle.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// Gets or sets the top edge of the rectangle.
        /// </summary>
        public int Top { get; set; }
    }
}
