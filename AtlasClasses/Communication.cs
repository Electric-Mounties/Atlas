//File:         Communication.cs
//Description:  This handles messaging back and forth between clients and this server, including encryption, keys and information
//Programmers:  Jordan Poirier, Thom Taylor, Matthew Thiessen, Tylor McLaughlin
//Date:         5/1/2015

using System;


namespace AtlasClasses
{
    public class Communication
    {
        public Communication()
        {
        }

        /// <summary>
        /// Temporary Method, Represents the threading needed for multi-client
        /// </summary>
        public void Threads()
        {

        }

        /// <summary>
        /// This sends a message to a specific client
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="clientID">id of client to talk to</param>
        public void Send(string message, int clientID)
        {

        }

        /// <summary>
        /// Sends a message to all clients
        /// </summary>
        /// <param name="message">message to send</param>
        public void Broadcast(string message)
        {

        }

        /// <summary>
        /// Event handler for receiving a message from a client
        /// </summary>
        /// <param name="Sender"></param>
        public void onReceive(object Sender)
        {

        }
    }
}
