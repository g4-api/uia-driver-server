/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents a point in a two-dimensional coordinate system.
    /// </summary>
    /// <param name="xpos">The x-coordinate of the point.</param>
    /// <param name="ypos">The y-coordinate of the point.</param>
    public class PointModel(int xpos, int ypos)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointModel"/> class with default coordinates (0, 0).
        /// </summary>
        public PointModel()
            : this(0, 0)
        { }

        /// <summary>
        /// Gets or sets the x-coordinate of the point.
        /// </summary>
        public int X { get; set; } = xpos;

        /// <summary>
        /// Gets or sets the y-coordinate of the point.
        /// </summary>
        public int Y { get; set; } = ypos;
    }
}
