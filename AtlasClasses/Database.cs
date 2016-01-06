//File:         Database.cs
//Description:  This class interfaces with the database, storing and retreiving information
//Programmers:  Jordan Poirier, Thom Taylor, Matthew Thiessen, Tylor McLaughlin
//Date:         5/1/2016

using System;

namespace AtlasClasses
{
    public class Database
    {
        public Database()
        {
        }

        /// <summary>
        /// Writes to the database
        /// </summary>
        /// <returns>True if write successful</returns>
        public bool write(string data)
        {
            return true;
        }

        /// <summary>
        /// Reads from the database
        /// </summary>
        /// <returns>data read from database</returns>
        public string read(string query)
        {
            return "";
        }


    }
}