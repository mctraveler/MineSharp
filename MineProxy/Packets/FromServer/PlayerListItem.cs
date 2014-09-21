using System;
using MiscUtil.IO;
using System.Collections.Generic;
using System.Threading;

namespace MineProxy.Packets
{
    public class PlayerListItem : PacketFromServer
    {
        public const byte ID = 0x38;

        public override byte PacketID { get { return ID; } }

        public Actions Action { get; set; }

        public List<PlayerItem> Players { get; set; }

        public enum Actions
        {
            AddPlayer = 0,
            UpdateGamemode = 1,
            UpdateLatency = 2,
            UpdateDisplayName = 3,
            RemovePlayer = 4,
        }

        public class PlayerItem
        {
            public Guid UUID { get; set; }

            public string Name { get; set; }

            public GameMode Gamemode { get; set; }

            public int Ping { get; set; }

            public PlayerItem()
            {
            }

            public PlayerItem(Guid uuid, string name, GameMode mode, int ping)
            {
                this.UUID = uuid;
                this.Name = name;
                this.Gamemode = mode;
                this.Ping = ping;
            }

            public PlayerItem(Guid uuid)
            {
                this.UUID = uuid;
            }

            public override string ToString()
            {
                return string.Format("[PlayerItem: UUID={0}, Name={1}, Gamemode={2}, Ping={3}]", UUID, Name, Gamemode, Ping);
            }
        }

        public override string ToString()
        {
            return string.Format("[PlayerListItem: Action={1}, Players={2}]", PacketID, Action, Players.Count);
        }

        public PlayerListItem()
        {
            Players = new List<PlayerItem>();
        }

        public PlayerListItem(Actions action)
        {
            Players = new List<PlayerItem>();
            Action = action;
        }

        public void AddPlayer(Guid guid, string name, int ping)
        {
            Players.Add(new PlayerItem(guid, name, GameMode.Survival, ping));
        }

        public void AddPlayer(Guid guid, string name)
        {
            Players.Add(new PlayerItem(guid, name, GameMode.Survival, 0));
        }

        public void RemovePlayer(Guid guid)
        {
            Players.Add(new PlayerItem(guid));
        }

        public static PlayerListItem RemovePlayer(Client pp)
        {
            var p = new PlayerListItem();
            p.Action = Actions.RemovePlayer;
            p.Players.Add(new PlayerItem(pp.UUID));
            return p;
        }

        //Only parsed so we can block it
        protected override void Parse(EndianBinaryReader r)
        {
            //Console.WriteLine("Player List Item:");
            //Console.WriteLine(BitConverter.ToString(PacketBuffer));
            return; //Ignore the data, we just want to block the package
            #if !DEBUG
            #else
            #pragma warning disable 162
            Action = (Actions)ReadVarInt(r);
            int count = ReadVarInt(r);
            Players = new List<PlayerItem>(count);
            switch (Action)
            {
                case Actions.AddPlayer:
                    for (int n = 0; n < count; n++)
                    {
                        var i = new PlayerItem();
                        i.UUID = new Guid(r.ReadBytesOrThrow(16));
                        i.Name = ReadString8(r);
                        int props = ReadVarInt(r);
                        for (int p = 0; p < props; p++)
                        {
                            string name = ReadString8(r);
                            string value = ReadString8(r);
                            bool signed = r.ReadBoolean();
                            if (signed)
                            {
                                string sign = ReadString8(r);
                            }
                            throw new NotImplementedException();
                        }
                        i.Gamemode = (GameMode)ReadVarInt(r);
                        i.Ping = ReadVarInt(r);
                        Players.Add(i);
                    }
                    break;
                case Actions.UpdateGamemode:
                    for (int n = 0; n < count; n++)
                    {
                        var i = new PlayerItem();
                        i.UUID = new Guid(r.ReadBytesOrThrow(16));
                        i.Gamemode = (GameMode)ReadVarInt(r);
                        Players.Add(i);
                    }
                    break;
                case Actions.UpdateLatency:
                    for (int n = 0; n < count; n++)
                    {
                        var i = new PlayerItem();
                        i.UUID = new Guid(r.ReadBytesOrThrow(16));
                        i.Ping = ReadVarInt(r);
                        Players.Add(i);
                    }
                    break;
                case Actions.RemovePlayer:
                    for (int n = 0; n < count; n++)
                    {
                        var i = new PlayerItem();
                        i.UUID = new Guid(r.ReadBytesOrThrow(16));
                        Players.Add(i);
                    }
                    break;

                default:
                    throw new NotImplementedException("Action: " + Action);
            }
            #pragma warning restore 162
            #endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            WriteVarInt(w, (int)Action);
            WriteVarInt(w, Players.Count);
            switch (Action)
            {
                case Actions.AddPlayer:
                    foreach (var i in Players)
                    {
                        w.Write(i.UUID.ToByteArray());
                        if (i.Name.Length <= 16)
                            WriteString8(w, i.Name);
                        else
                            WriteString8(w, i.Name.Substring(0, 16));
                        WriteVarInt(w, 0); //Array of properties
                        WriteVarInt(w, (int)i.Gamemode);
                        WriteVarInt(w, i.Ping);
                        w.Write(false);//Has display name
                        //If has display name, send chatjson
                    }
                    break;
                case Actions.UpdateGamemode:
                    foreach (var i in Players)
                    {
                        w.Write(i.UUID.ToByteArray());
                        WriteVarInt(w, (int)i.Gamemode);
                    }
                    break;
                case Actions.UpdateLatency:
                    foreach (var i in Players)
                    {
                        w.Write(i.UUID.ToByteArray());
                        WriteVarInt(w, i.Ping);
                    }
                    break;
                case Actions.UpdateDisplayName:
                    foreach (var i in Players)
                    {
                        w.Write(i.UUID.ToByteArray());
                        w.Write(false);//Has display name
                        //If has display name, send chatjson
                    }
                    break;
                case Actions.RemovePlayer:
                    foreach (var i in Players)
                    {
                        w.Write(i.UUID.ToByteArray());
                    }
                    break;

                default:
                    throw new NotImplementedException("Action: " + Action);
            }
        }

    }
}

