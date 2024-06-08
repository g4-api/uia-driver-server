/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents the input model for executing a script.
    /// </summary>
    public class ScriptInputModel
    {
        /// <summary>
        /// Gets or sets the parameters for the script.
        /// </summary>
        /// <value>A collection of parameters to be passed to the script.</value>
        public IEnumerable<object> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the script to be executed.
        /// </summary>
        /// <value>The script code to be executed.</value>
        [Required]
        public string Script { get; set; }
    }
}
