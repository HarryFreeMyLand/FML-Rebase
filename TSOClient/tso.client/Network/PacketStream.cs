﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSO LoginServer.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using TSOClient.Network.Encryption;
using LogThis;

namespace TSOClient.Network
{
    public class PacketStream : Stream
    {
        //The ID of this PacketStream (identifies a packet).
        private byte m_ID;
        //The intended length of this PacketStream. Might not correspond with the
        //length of m_BaseStream!
        private int m_Length;
        public bool m_VariableLength;

        private MemoryStream m_BaseStream;
        private bool m_SupportsPeek = false;
        private byte[] m_PeekBuffer;
        private BinaryReader m_Reader;
        private BinaryWriter m_Writer;
        private long m_Position;

        public PacketStream(byte ID, int Length, byte[] DataBuffer)
            : base()
        {
            m_ID = ID;
            m_Length = Length;

            m_BaseStream = new MemoryStream(DataBuffer);

            m_SupportsPeek = true;
            m_PeekBuffer = new byte[DataBuffer.Length];
            DataBuffer.CopyTo(m_PeekBuffer, 0);
            
            m_Reader = new BinaryReader(m_BaseStream);
            m_Position = DataBuffer.Length;
        }

        public PacketStream(byte ID, int Length)
        {
            m_ID = ID;
            m_Length = Length;

            m_SupportsPeek = false;

            m_BaseStream = new MemoryStream();
            m_Writer = new BinaryWriter(m_BaseStream);
            m_Position = 0;
        }

        public PacketStream(byte ID, int Length, bool VariableLength) : this(ID, Length)
        {
            this.m_VariableLength = VariableLength;
        }

        public bool VariableLength
        {
            get{
                return m_VariableLength;
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public bool CanPeek
        {
            get { return m_SupportsPeek; }
        }

        public byte PacketID
        {
            get { return m_ID; }
        }

        /// <summary>
        /// The current position of this PacketStream.
        /// </summary>
        public override long Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                //TODO: Checks here?
                m_Position = value;
            }
        }

        /// <summary>
        /// The target length of this PacketStream.
        /// To get the actual current length, use the BufferLength property.
        /// </summary>
        public override long Length
        {
            get { return m_Length; }
        }

        /// <summary>
        /// The current length of this PacketStream.
        /// </summary>
        public long BufferLength
        {
            get { return m_BaseStream.Length; }
        }

        /// <summary>
        /// Sets the length of this PacketStream to the specified value.
        /// </summary>
        /// <param name="value">The length of the stream.</param>
        public override void SetLength(long value)
        {
            byte[] Tmp = m_BaseStream.ToArray();
            //No idea if these two lines actually work, but they should...
            m_BaseStream = new MemoryStream((int)value);
            m_BaseStream.Write(Tmp, 0, Tmp.Length);
        }

        /// <summary>
        /// Do not call this! It will throw a NotImplementedException!
        /// </summary>
        /// <param name="offset">The offset to seek to.</param>
        /// <param name="origin">The origin of the seek.</param>
        /// <returns>The offset that was seeked to.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            m_BaseStream.Flush();
        }

        public byte[] ToArray()
        {
            var bytes = m_BaseStream.ToArray();
            if (m_VariableLength)
            {
                var packetLength = (ushort)m_Position;
                bytes[2] = (byte)(packetLength & 0xFF);
                bytes[3] = (byte)(packetLength >> 8);
            }
            return bytes;
        }

        /// <summary>
        /// Decrypts the data in this PacketStream.
        /// WARNING: ASSUMES THAT THE 7-BYTE HEADER
        /// HAS BEEN READ (ID, LENGTH, DECRYPTEDLENGTH)!
        /// </summary>
        /// <param name="Key">The client's en/decryptionkey.</param>
        /// <param name="Service">The client's DESCryptoServiceProvider instance.</param>
        /// <param name="UnencryptedLength">The packet's unencrypted length (third byte in the header).</param>
        public void DecryptPacket(byte[] Key, DESCryptoServiceProvider Service, ushort UnencryptedLength)
        {
            CryptoStream CStream = new CryptoStream(m_BaseStream, Service.CreateDecryptor(Key,
                Encoding.ASCII.GetBytes("@1B2c3D4e5F6g7H8")), CryptoStreamMode.Read);

            byte[] DecodedBuffer = new byte[UnencryptedLength];
            CStream.Read(DecodedBuffer, 0, DecodedBuffer.Length);

            m_BaseStream = new MemoryStream(DecodedBuffer);
        }

        #region Reading

        /// <summary>
        /// Reads a specific number of bytes from this PacketStream.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The offset from which to start reading.</param>
        /// <param name="count">The number of bytes to read (must be at least equal to the length of buffer!)</param>
        /// <returns>The number of bytes that were read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int Read = m_BaseStream.Read(buffer, offset, count);
            m_Position -= Read;

            return Read;
        }

        /// <summary>
        /// Peeks a byte from the stream at the current position.
        /// </summary>
        /// <returns>The byte that was peeked.</returns>
        public byte PeekByte()
        {
            if (m_SupportsPeek)
                return m_PeekBuffer[m_Position];
            else
            {
                Log.LogThis("Tried peeking from a PacketStream instance that didn't support it!", eloglevel.warn);
                return 0;
            }
        }

        /// <summary>
        /// Peeks a byte from the stream at the specified position.
        /// </summary>
        /// <param name="Position">The position to peek at.</param>
        /// <returns>The byte that was peeked.</returns>
        public byte PeekByte(int Position)
        {
            if (m_SupportsPeek)
                return m_PeekBuffer[Position];
            else
            {
                Log.LogThis("Tried peeking from a PacketStream instance that didn't support it!", eloglevel.warn);
                return 0;
            }
        }

        /// <summary>
        /// Peeks a ushort from the stream at the specified position.
        /// </summary>
        /// <param name="Position">The position to peek at.</param>
        /// <returns>The ushort that was peeked.</returns>
        public ushort PeekUShort(int Position)
        {
            MemoryStream MemStream = new MemoryStream();
            BinaryWriter Writer = new BinaryWriter(MemStream);

            Writer.Write((byte)PeekByte(Position));
            Writer.Write((byte)PeekByte(Position + 1));
            Writer.Flush();

            return BitConverter.ToUInt16(MemStream.ToArray(), 0);
        }

        public override int ReadByte()
        {
            m_Position -= 1;
            return m_BaseStream.ReadByte();
        }

        public ushort ReadUShort()
        {
            m_Position -= 2;

            MemoryStream MemStream = new MemoryStream();
            BinaryWriter Writer = new BinaryWriter(MemStream);

            Writer.Write((byte)ReadByte());
            Writer.Write((byte)ReadByte());

            return BitConverter.ToUInt16(MemStream.ToArray(), 0);
        }

        public string ReadString()
        {
            string ReturnStr = m_Reader.ReadString();
            m_Position -= ReturnStr.Length;

            return ReturnStr;
        }

        public string ReadString(int NumChars)
        {
            string ReturnStr = "";

            for (int i = 0; i <= NumChars; i++)
                ReturnStr = ReturnStr + m_Reader.ReadChar();

            m_Position -= NumChars;

            return ReturnStr;
        }

        public int ReadInt32()
        {
            m_Position -= 4;
            return m_Reader.ReadInt32();
        }

        public long ReadInt64()
        {
            m_Position -= 8;
            return m_Reader.ReadInt64();
        }

        public ushort ReadUInt16()
        {
            m_Position -= 2;
            return m_Reader.ReadUInt16();
        }

        public ulong ReadUInt64()
        {
            m_Position -= 8;
            return m_Reader.ReadUInt64();
        }

        #endregion

        #region Writing


        /// <summary>
        /// Writes the packet header
        /// </summary>
        public void WriteHeader()
        {
            WriteUInt16(this.m_ID);
            if (m_VariableLength)
            {
                /** Leave 2 empty bytes to fill in during toArray() that will be the length **/
                WriteUInt16(0);
            }
        }

        /// <summary>
        /// Writes a block of bytes to the current buffer using data read from the buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            m_BaseStream.Write(buffer, offset, count);
            m_Position += count;
            m_Writer.Flush();
        }

        public void WriteBytes(byte[] Buffer)
        {
            m_BaseStream.Write(Buffer, 0, Buffer.Length);
            m_Position += Buffer.Length;
            m_Writer.Flush();
        }

        public override void WriteByte(byte Value)
        {
            m_Writer.Write(Value);
            m_Position += 1;
            m_Writer.Flush();
        }


        public void WriteInt32(int Value)
        {
            m_Writer.Write(Value);
            m_Position += 4;
            m_Writer.Flush();
        }

        public void WriteUInt16(ushort Value)
        {
            m_Writer.Write(Value);
            m_Position += 2;
            m_Writer.Flush();
        }

        public void WriteInt64(long Value)
        {
            m_Writer.Write(Value);
            m_Position += 8;
            m_Writer.Flush();
        }

        public void WriteASCII(string str)
        {
            WriteByte((byte)str.Length);
            WriteBytes(Encoding.ASCII.GetBytes(str));
        }

        #endregion
    }
}
