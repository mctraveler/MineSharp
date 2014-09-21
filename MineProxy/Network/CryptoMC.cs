using System;
using System.IO;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Asn1.X509;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;
using MineProxy.Packets;

namespace MineProxy.Network
{
    public class CryptoMC : Stream
    {
        Stream BaseStream;
        static RsaKeyParameters keyParameters;
        /// <summary>
        /// Clear-text Shared Key
        /// </summary>
        public byte[] SharedKey { get; set; }

        public byte[] SharedKeyEncrypted { get; private set; }
        public byte[] TestEncrypted { get; private set; }

        StreamBlockCipher encStream;
        StreamBlockCipher decStream;

        public static byte[] Init()
        {
            AsymmetricCipherKeyPair keyPair = null;

            string path = "proxy/session-key";
            try
            {
                AsymmetricKeyParameter keyPub = PublicKeyFactory.CreateKey(File.ReadAllBytes(path + ".pub"));
                AsymmetricKeyParameter KeyPriv = PrivateKeyFactory.CreateKey(File.ReadAllBytes(path + ".priv"));
                keyPair = new AsymmetricCipherKeyPair(keyPub, KeyPriv);
                Debug.WriteLine("Loaded session.key");
            } catch (Exception e)
            {
                Log.WriteServer(e);

                Debug.WriteLine("Generating session.key...");
                var generator = new RsaKeyPairGenerator();
                generator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
                keyPair = generator.GenerateKeyPair();
                Debug.WriteLine("Generated, saving...");

                SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
                byte[] bytePub = publicKeyInfo.ToAsn1Object().GetDerEncoded();
                File.WriteAllBytes(path + ".pub", bytePub);

                PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);                        
                byte[] bytePriv = privateKeyInfo.ToAsn1Object().GetDerEncoded();
                File.WriteAllBytes(path + ".priv", bytePriv);

                Debug.WriteLine("Saved session.key");

            }

            //Old method
            /*var generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            var keyPair = generator.GenerateKeyPair();
            */

            keyParameters = (RsaKeyParameters)keyPair.Private;
            SubjectPublicKeyInfo info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
            return info.GetEncoded();
        }

        /// <summary>
        /// Client side encryption
        /// </summary>
        public CryptoMC(Stream baseStream, EncryptionRequest req)
        {
            this.BaseStream = baseStream;
            RijndaelManaged rm = new RijndaelManaged();
            rm.KeySize = 128;
            rm.GenerateKey();
            SharedKey = rm.Key;
            InitCiphers();

            //Encrypt shared key using public key in req
            AsymmetricKeyParameter asymmetricKeyParameter = PublicKeyFactory.CreateKey(req.PublicKey);
            RsaKeyParameters key = (RsaKeyParameters)asymmetricKeyParameter;
            //SubjectPublicKeyInfo s = new SubjectPublicKeyInfo(AlgorithmIdentifier.Der, req.PublicKey);
            //RsaKeyParameters key = (RsaKeyParameters)PublicKeyFactory.CreateKey(s);

            Pkcs1Encoding padding = new Pkcs1Encoding(new RsaEngine());
            padding.Init(true, key);
            SharedKeyEncrypted = padding.ProcessBlock(SharedKey, 0, SharedKey.Length);

            Pkcs1Encoding padding2 = new Pkcs1Encoding(new RsaEngine());
            padding2.Init(true, keyParameters);
            TestEncrypted = padding.ProcessBlock(req.VerifyToken, 0, req.VerifyToken.Length);

        }

        /// <summary>
        /// Server side encryption
        /// </summary>
        public CryptoMC(Stream baseStream, EncryptionResponse resp)
        {
            this.BaseStream = baseStream;

            Pkcs1Encoding padding = new Pkcs1Encoding(new RsaEngine());
            padding.Init(false, keyParameters);
            SharedKey = padding.ProcessBlock(resp.SharedKey, 0, resp.SharedKey.Length);

            InitCiphers();
        }

        private void InitCiphers()
        {
            CfbBlockCipher encryptor = new CfbBlockCipher(new AesEngine(), 8);
            CfbBlockCipher decryptor = new CfbBlockCipher(new AesEngine(), 8);
            encryptor.Init(true, new ParametersWithIV(new KeyParameter(SharedKey), SharedKey));
            decryptor.Init(false, new ParametersWithIV(new KeyParameter(SharedKey), SharedKey));
            encStream = new StreamBlockCipher(encryptor);
            decStream = new StreamBlockCipher(decryptor);
        }

		#region implemented abstract members of System.IO.Stream
        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte [] b = new byte[count];
            int read = BaseStream.Read(b, 0, count);
            decStream.ProcessBytes(b, 0, read, buffer, offset);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] b = new byte[count];
            encStream.ProcessBytes(buffer, offset, count, b, 0);
            BaseStream.Write(b, 0, count);
        }

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override long Length
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }
		#endregion
    }
}

