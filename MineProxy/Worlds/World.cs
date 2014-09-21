using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using MineProxy.Packets;

namespace MineProxy.Worlds
{
    /// <summary>
    /// Dimension, either to the vanilla server or locally hosted
    /// </summary>
    public abstract class World
    {
        public CommandManager Commands { get; set; }

        /// <summary>
        /// Null if regions are disabled
        /// </summary>
        public RegionList Regions;

        readonly List<WorldSession> players = new List<WorldSession>();
        public WorldSession[] Players { get; private set; }

        public World()
        {
            Players = new WorldSession[0];
        }

        /// <summary>
        /// Player enters the world
        /// </summary>
        public abstract WorldSession Join(Client player);

        public virtual void Send(string commands)
        {
            //throw new InvalidOperationException("Not a vanilla world");
        }

        public static readonly VanillaWorld Main = new VanillaWorld("main");
        public static readonly AfkWorld AFK = new AfkWorld();
        public static readonly GreenRoom Wait = new GreenRoom();
        public static readonly Hell HellBanned = new Hell();
        public static readonly TheConstruct Construct = new TheConstruct();
        public static readonly Void Void = new Void();
        public static readonly PossessWorld Possess = new PossessWorld();
        public static readonly Dictionary<string, VanillaWorld> VanillaWorlds = new Dictionary<string, VanillaWorld>();

        static World()
        {
            var dirs = Directory.GetDirectories(Directory.GetCurrentDirectory());
            foreach (string d in dirs)
            {
                try
                {
                    string name = Path.GetFileName(d);
                    VanillaWorld w = new VanillaWorld(name);
                    VanillaWorlds.Add(w.ServerName, w);
                    Console.WriteLine("Loaded VanillaWorld: " + w);
                } catch (Exception)
                {
                }
            }

            Main = VanillaWorlds ["main"];
        }

        protected void Join(WorldSession cs)
        {
            lock (players)
            {
                players.Add(cs);
                Players = players.ToArray();
            }
        }

        public void Leave(WorldSession session)
        {
            lock (players)
            {
                players.Remove(session);
                Players = players.ToArray();
            }
        }

        public void SendToAll(PacketFromServer packet)
        {
            foreach (var p in Players)
            {
                p.Send(packet);
            }
        }

        public void SendToAll(List<PacketFromServer> packets)
        {
            if (packets.Count == 0)
                return;

            var pre = new PrecompiledPacket(packets);

            foreach (var p in Players)
            {
                p.Send(pre);
            }
        }

        public void Say(string prefix, string message)
        {
            foreach (var cs in Players)
            {
                cs.TellSystem(prefix, message);
            }
        }
    }
}

