using System;
using MiscUtil.IO;
using System.Net.Sockets;
using System.IO;
using MiscUtil.Conversion;
using MineProxy.Packets;
using MineProxy.Network;
using MineProxy.Misc;
using MineProxy.Worlds;

namespace MineProxy.Clients
{
    public partial class VanillaClient : Client
    {
        /// <summary>
        /// -1 until set
        /// </summary>
        int maxUncompressed = -1;

        public VanillaClient(Socket socket)
            : base(socket, new NetworkStream(socket))
        {
        }

        public override void Close(string message)
        {
            clientStream.Flush();
            base.Close(message);
        }

        protected override void ReceiverRunClient()
        {
            try
            {
                RunClientHandshake();
                if (Phase == Phases.FinalClose)
                    return;
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (IOException)
            {
                return;
            }
            catch (ProtocolException)
            {
                return;
            }
            #if !DEBUG
            catch (Exception e) {
				Log.Write (e, this);
				return;
			}
            #endif

            if (Phase == Phases.FinalClose)
                return;
            if (Phase != Phases.Gaming)
                throw new ProtocolException("Expected gaming, phase was " + Phase);
            if (MinecraftUsername == null)
                throw new ProtocolException("Missing username");
            clientThread.State = "Gaming";

#if !DEBUG
			Packet prev = null;
#endif
            PacketFromClient p = null;

            while (true)
            {
                //Read packet
                try
                {
                    if (Phase == Phases.FinalClose)
                        return;

                    #if !DEBUG
					prev = p;                        
                    #endif
                    clientThread.WatchdogTick = DateTime.Now;

                    p = PacketFromClient.ReadClient(clientStream);

                    Debug.FromClient(this, p);

                }
                catch (EndOfStreamException)
                {
                    return;
                }
                catch (IOException)
                {
                    return;
                    #if !DEBUG
				} catch (Exception e) {
					if (Phase != Phases.FinalClose) {
						if (prev != null)
							Log.Write (new PrevException (prev), this);
						Log.Write (e, this);
					}
					return;
                    #endif
                }

                #if DEBUG
                //Console.WriteLine("From client: "+p);
                #endif

                if (Phase == Phases.FinalClose)
                    return;

                //Handle packet
                try
                {
                    FromClient(p);
                }
                catch (SessionClosedException)
                {
                    return;
                }
                #if !DEBUG
                catch (Exception e) {
					if (Phase == Phases.FinalClose)
						return;
					Log.Write (e, this);
					return;
				}
                #endif
            }
        }

        protected override void SendToClientInternal(PacketFromServer packet)
        {
            try
            {
                if (packet.PacketBuffer == null)
                    packet.Prepare();

                Debug.ToClient(this, packet);

                if (packet is PrecompiledPacket)
                {
                    lock (clientStream)
                        clientStream.Write(packet.PacketBuffer, 0, packet.PacketBuffer.Length);
                    return;
                }

                lock (clientStream)
                {
                    if(maxUncompressed >= 0)
                    {
                        Packet.WriteVarInt(clientStream, packet.PacketBuffer.Length + Packet.VarIntSize(packet.PacketBufferUncompressedSize)); //Uncompressed size length
                        Packet.WriteVarInt(clientStream, packet.PacketBufferUncompressedSize); //Uncompressed size if compressed
                    }
                    else
                        Packet.WriteVarInt(clientStream, packet.PacketBuffer.Length);

                    clientStream.Write(packet.PacketBuffer, 0, packet.PacketBuffer.Length);
                    #if DEBUG
                    clientStream.Flush();
                    #endif
                }
            }
            catch (SocketException se)
            {
                Phase = Phases.FinalClose;
                Close(se.Message);
            }
            catch (IOException ioe)
            {
                Phase = Phases.FinalClose;
                Close(ioe.Message);
            }
            catch (ObjectDisposedException)
            {
                Phase = Phases.FinalClose;
            }
        }
    }
}

