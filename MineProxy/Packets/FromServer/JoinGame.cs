using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class JoinGame : PacketFromServer
    {
        public const int ID = 0x01;

        public override byte PacketID { get { return ID; } }

        /// <summary>
        /// Client only
        /// </summary>
        public ProtocolVersion ClientVersion { get; set; }

        /// <summary>
        /// Server Only
        /// </summary>
        public int EntityID { get; set; }

        /// <summary>
        /// 0 Survival, 1 Creative
        /// </summary>
        public GameMode GameMode { get; set; }

        public Dimensions Dimension { get; set; }

        /// <summary>
        /// 0 thru 3 for Peaceful, Easy, Normal, Hard
        /// </summary>
        public byte Difficulty { get; set; }

        public byte MaxPlayers { get; set; }

        public string LevelType { get; set; }

        /// <summary>
        /// Reduced Debug Info
        /// </summary>
        public bool ReducedDebug { get; set; }

        public override string ToString()
        {
            return string.Format("[LoginRequest: ProtocolVersion={0}, EntityID={1}, ServerMode={2}, Dimension={3}, Difficulty={4}, MaxPlayers={5}]", ClientVersion, EntityID, GameMode, Dimension, Difficulty, MaxPlayers);
        }

        public JoinGame()
        {
            LevelType = "default";
        }

        public JoinGame(byte[] buffer)
        {
            SetPacketBuffer(buffer);
            Parse();
        }

        protected override void Parse(EndianBinaryReader r)
        {
            EntityID = r.ReadInt32();
            GameMode = (GameMode)r.ReadByte();
            Dimension = (Dimensions)r.ReadSByte();
            Difficulty = r.ReadByte();
            MaxPlayers = r.ReadByte();
            LevelType = ReadString8(r);
            ReducedDebug = r.ReadBoolean();
            #if DEBUGPACKET
            if (Dimension.ToString() == ((int)Dimension).ToString())
                throw new NotImplementedException(Dimension.ToString());
            if (GameMode.ToString() == ((int)GameMode).ToString())
                throw new NotImplementedException(GameMode.ToString());
            #endif
        }

        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((int)EntityID);
            w.Write((byte)GameMode);
            w.Write((sbyte)Dimension);
            w.Write((byte)Difficulty);
            w.Write((byte)MaxPlayers);
            WriteString8(w, LevelType);
            w.Write((bool)ReducedDebug);
        }
		
    }
}

