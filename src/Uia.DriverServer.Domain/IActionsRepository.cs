/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Uia.DriverServer.Models;

namespace Uia.DriverServer.Domain
{
    /// <summary>
    /// Defines the methods for handling and sending actions to the system.
    /// </summary>
    public interface IActionsRepository
    {
        /// <summary>
        /// Sends a series of actions to the system based on the provided session and actions model.
        /// </summary>
        /// <param name="session">The session model containing session details.</param>
        /// <param name="actionsModel">The actions model containing the actions to be sent.</param>
        void SendActions(UiaSessionResponseModel session, ActionsModel actionsModel);
    }
}
