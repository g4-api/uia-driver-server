/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Swashbuckle.AspNetCore.Annotations;

using System.ComponentModel.DataAnnotations;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the input model for specifying a mouse position.
    /// </summary>
    public class MousePositionInputModel
    {
        /// <summary>
        /// Gets or sets the alignment for the mouse position.
        /// </summary>
        /// <value>The alignment for the mouse position (e.g., TopLeft, MiddleCenter).</value>
        [SwaggerSchema("The alignment for the mouse position (e.g., TopLeft, MiddleCenter).")]
        [SwaggerParameter(Description = "Allowed values: TopLeft, MiddleLeft, BottomLeft, TopCenter, MiddleCenter, BottomCenter, TopRight, MiddleRight, BottomRight.")]
        [RegularExpression(
            pattern:"TopLeft|MiddleLeft|BottomLeft|TopCenter|MiddleCenter|BottomCenter|TopRight|MiddleRight|BottomRight",
            ErrorMessage = "Invalid alignment value. Allowed values: TopLeft, MiddleLeft, BottomLeft, TopCenter, MiddleCenter, BottomCenter, TopRight, MiddleRight, BottomRight.")]
        public string Alignment { get; set; }

        /// <summary>
        /// Gets or sets the offset from the X-coordinate.
        /// </summary>
        /// <value>The offset from the X-coordinate.</value>
        public int OffsetX { get; set; }

        /// <summary>
        /// Gets or sets the offset from the Y-coordinate.
        /// </summary>
        /// <value>The offset from the Y-coordinate.</value>
        public int OffsetY { get; set; }

        /// <summary>
        /// Gets or sets the X-coordinate of the mouse position.
        /// </summary>
        /// <value>The X-coordinate of the mouse position.</value>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the Y-coordinate of the mouse position.
        /// </summary>
        /// <value>The Y-coordinate of the mouse position.</value>
        public int Y { get; set; }
    }
}
