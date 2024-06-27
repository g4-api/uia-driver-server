/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System.Collections.Generic;

namespace Uia.DriverServer.Models
{
    /// <summary>
    /// Represents a model for a collection of actions.
    /// </summary>
    public class ActionsModel
    {
        /// <summary>
        /// Gets or sets the collection of actions.
        /// </summary>
        public IEnumerable<ActionModel> Actions { get; set; }

        /// <summary>
        /// Represents an individual action model.
        /// </summary>
        public class ActionModel
        {
            /// <summary>
            /// Gets or sets the collection of action data models.
            /// </summary>
            public IEnumerable<Dictionary<string, object>> Actions { get; set; }

            /// <summary>
            /// Gets or sets the identifier for the action.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the type of the action.
            /// </summary>
            public string Type { get; set; }
        }
    }
}
