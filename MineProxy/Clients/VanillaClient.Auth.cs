using System;
using System.Net;
using MineProxy.Network;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.IO;
using MineProxy.Worlds;
using MineProxy.Packets;
using MineProxy.Misc;

namespace MineProxy.Clients
{
    public partial class VanillaClient
    {
        //For auth
        string unverifiedUsername;
        byte[] ID;
        static int freeEID = 5000000;

        public override string ToString()
        {
            #if DEBUG
            if (Settings.Nick == null)
                return string.Format(
                    "{0}({1})",
                    MinecraftUsername,
                    EntityID
                );
            else
                return string.Format(
                    "{0} AKA {1} ({2})",
                    MinecraftUsername,
                    Settings.Nick,
                    EntityID
                );
            #else
            return "Client";
            #endif
        }

        /// <summary>
        /// Return false if disconnect/finished
        /// </summary>
        void RunClientHandshake()
        {
            clientThread.WatchdogTick = DateTime.Now;

            //Read Handshake
            var hs = PacketReader.ReadFirstPackage(clientStream);
            if (hs == null)
            {
                //Old status packet
                LegacyStatus.SendStatus(clientStream);
                Phase = Phases.FinalClose;
                return;
            }
            var h = new Handshake(hs);
            Debug.FromClient(this, h);
            clientThread.State = "Handshake Received " + h;
            ClientVersion = h.Version;
            if (h.State == HandshakeState.Status)
            {
                RunStatusPing(h);
                return;
            }
            if (h.State == HandshakeState.None)
            {
                Close("Invalid handshake state: " + h.State);
                return;
            }
            if (h.State != HandshakeState.Login)
            {
                Close("Invalid handshake state: " + h.State);
                return;
            }

            #if DEBUG
            if (h.Version >= ProtocolVersion.next)
                throw new InvalidDataException("new version: " + h.Version);
            #endif
            if (h.Version > MinecraftServer.BackendVersion)
            {
                clientThread.State = "Handshake too high version";
                Close("We are still running " + MinecraftServer.BackendVersion.ToText() + " we are so very sorry about that");
                return;
            }
            if (h.Version < MinecraftServer.FrontendVersion)
            {
                clientThread.State = "Handshake too low version";
                Close("Upgrade your client to " + MinecraftServer.FrontendVersion.ToText());
                return;
            }

            clientThread.WatchdogTick = DateTime.Now;

            //Login
            var l = new LoginStart(PacketReader.ReadHandshake(clientStream));
            Debug.FromClient(this, l);

            clientThread.State = "LoginStart Received " + l.Name;
            unverifiedUsername = l.Name;
            if (unverifiedUsername.Length == 0 || unverifiedUsername.Length > 16)
            {
                clientThread.State = "Handshake wrong username length";
                Close("Bad username");
                Console.WriteLine("Wrong username length: " + unverifiedUsername.Length);
                return;
            }

            //Set Compression
            var compression = new SetCompression();
            SendToClientInternal(compression);
            maxUncompressed = compression.MaxSize;

            //Send encryption request
            clientThread.State = "Handshake: Sending encryption request";
            //Initiate ID
            var r = new Random();
            ID = new byte[8];
            r.NextBytes(ID);
            SendToClientInternal(new EncryptionRequest(Authentication.McHex(ID), MinecraftServer.RsaBytes));
            clientThread.State = "Handshake: Sent encryption request, reading response";

            //Read enc response
            int erSize;
            var erBuffer = PacketReader.Read(clientStream, out erSize);
            if (erSize != 0)
                erBuffer = Compression.Decompress(erBuffer, erSize);
            var er = new EncryptionResponse(erBuffer);
            Debug.FromClient(this, er);

            clientThread.State = "Handshake: Got enc resp";
            CryptoMC cryptoStream = new CryptoMC(clientStream, er);

            //Verify user
            clientThread.State = "Handshake: loading proxy data";
            Settings = LoadProxyPlayer(unverifiedUsername);

            clientThread.State = "Handshake: Verifying login credentials";
            var auth = Authentication.VerifyUserLogin(unverifiedUsername, cryptoStream.SharedKey, ID);
            #warning From here we now need to take care of the id in the response.
            if (auth == null)
            {
                //Unauthorized
                Log.WriteAuthFail(unverifiedUsername, RemoteEndPoint, "");
                Close("Mojang says no");
                return;
            }
            else
            {
                MinecraftUsername = unverifiedUsername;
                Log.WritePlayer(this, "Login from " + RemoteEndPoint + " " + Country);
            }
            clientThread.State = "Handshake: Logged in";

            //Get UUID
            Settings.UUID = auth.id;

            clientThread.WatchdogTick = DateTime.Now;

            MineProxy.Inbox.Status(this);

            //start encryption
            clientStream = cryptoStream;

            SendToClientInternal(new LoginSuccess(Settings.UUID, MinecraftUsername));

            clientThread.User = MinecraftUsername;
            clientThread.State = "Logged in";

            EntityID = freeEID;
            
            //Math.Abs(unverifiedUsername.GetHashCode());
            freeEID += 5000;

            //Login reply to client
            //No longer, send the vanilla server Login reply instead.

            Phase = Phases.Gaming;
            Queue = new SendQueue(this);

            PlayerList.LoginPlayer(this);

            SaveProxyPlayer();

            string name = h.Host.ToLowerInvariant().Split('.')[0];
            if (World.VanillaWorlds.ContainsKey(name))
                SetWorld(World.VanillaWorlds[name]);
            else
                SetWorld(World.Main);
            //Handshake complete
        }

        void RunStatusPing(Handshake h)
        {
            //C->S : Handshake State=1
            Debug.WriteLine("Status: " + h);

            //C->S : Request, 1 byte with value 0x00
            int size = clientStream.ReadByte();
            if (size != 1)
                throw new InvalidOperationException();
            int id = clientStream.ReadByte();
            if (id != 0x00)
                throw new ProtocolException("Expected ServerStatusRequest.ID 0x00 got " + id);
            Debug.WriteLine("Status: got request");

            //Debug.FromClient(this, new ServerStatusRequest());
            //S->C : Response
            SendToClientInternal(new ServerStatusResponse(new ServerStatus()));
            Debug.WriteLine("Status: sent response");

            //C->S : Ping
            //S->C : Ping
            var ping = new ServerPing(PacketReader.ReadHandshake(clientStream));
            Debug.WriteLine("Status: " + ping);
            SendToClientInternal(new ServerPong(ping.Time));
            Debug.WriteLine("Status: ping sent");
            Phase = Phases.FinalClose;
            Debug.WriteLine("Status: done closing");
        }

        bool Equal(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int n = 0; n < a.Length; n++)
            {
                if (a[n] != b[n])
                    return false;
            }
            return true;
        }
    }
}

