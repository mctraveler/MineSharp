using System;
using MiscUtil.IO;

namespace MineProxy.Packets
{
    public class ChangeGameState : PacketFromServer
    {
        public const byte ID = 0x2B;

        public override byte PacketID { get { return ID; } }
		
        public GameState Reason { get; set; }
		
        public float Value { get; set; }
		
        public override string ToString()
        {
            return string.Format("[NewState: Reason={1}, Mode={2}]", PacketID, Reason, Value);
        }
		
        public ChangeGameState(GameState reason)
        {
            if (reason == GameState.ChangeGameMode)
                throw new InvalidOperationException("ChangeGameMode must use other constructor");
            this.Reason = reason;
        }
		
        public static ChangeGameState ChangeGameMode(GameMode mode)
        {
            ChangeGameState n = new ChangeGameState();
            n.Reason = GameState.ChangeGameMode;
            n.Value = (float)mode;
            return n;
        }
		
        public ChangeGameState()
        {
        }

        protected override void Parse(EndianBinaryReader r)
        {
            Reason = (GameState)r.ReadByte();
            Value = r.ReadSingle();
            #if DEBUGPACKET
            if (Reason.ToString() == ((int)Reason).ToString())
                throw new NotImplementedException(Reason.ToString());
            #endif
        }
		
        protected override void Prepare(EndianBinaryWriter w)
        {
            w.Write((byte)Reason);
            w.Write((float)Value);			
        }
    }
}

