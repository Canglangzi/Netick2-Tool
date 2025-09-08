
using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace CocKleBurs.GameFrameWork.LagCompensation
{
    /// <summary>
    ///     Handles lag compensation
    /// </summary>
    [Preserve]
    public static class LagCompensationManager
    {
        private static List<LagCompensatedObject> lagCompensatedObjects;

        /// <summary>
        ///     Sets up the server side of <see cref="LagCompensationManager" />
        /// </summary>
        internal static void ServerSetup()
        {
            lagCompensatedObjects = new List<LagCompensatedObject>();
        }

        /// <summary>
        ///     Shuts down the server side of <see cref="LagCompensationManager" />
        /// </summary>
        internal static void ServerShutdown()
        {
            lagCompensatedObjects?.Clear();
            lagCompensatedObjects = null;
        }

        /// <summary>
        ///     Add a <see cref="LagCompensatedObject" /> to this manager
        /// </summary>
        /// <param name="obj"></param>
        internal static void AddLagCompensatedObject(LagCompensatedObject obj)
        {
            lagCompensatedObjects.Add(obj);
        }

        /// <summary>
        ///     Removes a <see cref="LagCompensatedObject" /> from this manager
        /// </summary>
        /// <param name="obj"></param>
        internal static void RemoveLagCompensatedObject(LagCompensatedObject obj)
        {
            lagCompensatedObjects.Remove(obj);
        }

        /// <summary>
        ///     Adds a frame to every lag compensated object
        /// </summary>
        internal static void ServerUpdate()
        {
            for (int i = 0; i < lagCompensatedObjects.Count; i++)
                lagCompensatedObjects[i].AddFrame();
        }

        /// <summary>
        ///     Simulates an <see cref="Action" /> for a <see cref="PlayerManager" />
        /// </summary>
        /// <param name="playerExecutedCommand"></param>
        /// <param name="command"></param>
        // public static void Simulate(PlayerManager playerExecutedCommand, Action command)
        // {
        //     double playerLatency = PingManager.GetClientPing(playerExecutedCommand.connectionToClient.connectionId);
        //
        //     Simulate(playerLatency, command);
        // }

        /// <summary>
        ///     Simulates an <see cref="Action" />
        /// </summary>
        /// <param name="secondsAgo"></param>
        /// <param name="command"></param>
        public static void Simulate(double secondsAgo, Action command)
        {
            for (int i = 0; i < lagCompensatedObjects.Count; i++)
                lagCompensatedObjects[i].SetStateTransform(secondsAgo);

            try
            {
                command();
            }
            catch (Exception ex)
            {
              //  Logger.Error(ex, "Error handing simulation of command!");
            }

            for (int i = 0; i < lagCompensatedObjects.Count; i++) lagCompensatedObjects[i].ResetStateTransform();
        }
    }
}