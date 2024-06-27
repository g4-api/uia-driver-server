/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using System;
using System.Linq;
using System.Threading;

using Uia.DriverServer.Extensions;
using Uia.DriverServer.Marshals;
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Implements the IActionsRepository interface to handle sending actions to the system.
    /// </summary>
    public class ActionsRepository : IActionsRepository
    {
        /// <inheritdoc />
        public void SendActions(UiaSessionResponseModel session, ActionsModel actions)
        {
            // Flatten the nested actions into a single sequence of actions.
            var inputs = actions
                .Actions
                .SelectMany(i => i.Actions);

            // Iterate through each input in the sequence.
            foreach (var input in inputs)
            {
                // If the input type is not "pause", convert it to keyboard input events and send them.
                if ($"{input["type"]}" != "pause")
                {
                    var inputsArray = input.ConvertToInputs().ToArray();
                    User32.SendInput(inputsArray);
                }

                // Determine the duration to sleep based on the input type.
                var duration = input.TryGetValue("duration", out object value) && value != null && value is long
                    ? (double)value : 10D;

                // Sleep for the specified duration.
                Thread.Sleep(TimeSpan.FromMilliseconds(duration));
            }
        }
    }
}
