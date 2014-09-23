using System;

namespace MineSharp
{
    class Settings
    {
        /// <summary>
        /// This is the path to the base location for the minecraft worlds, log files and other settings.
        /// Each world will have its own subdirectory to this, the default wolrd will be in "main".
        /// </summary>
        public const string BaseWorldsPath = "/home/traveler";

        /// <summary>
        /// The full path to minecraft_server.jar
        /// </summary>
        public const string MinecraftServerJar = "/home/bin/minecraft_server.jar";
    }
}