/*
    This file is part of Armored Deathmatch by Hans Milling.

    Armored Deathmatch is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Armored Deathmatch is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Armored Deathmatch.  If not, see <http://www.gnu.org/licenses/>.
	
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Server
{
  /// <summary>
  /// This class is used for network communication
  /// </summary>
  class Frame
  {
    public enum Opcode { Continuation = 0, Text, Binary, Res1, Res2, Res3, Res4, Res5, Close, Ping, Pong, Res6, Res7, Res8, Res9, Disconnect };
    private List<byte> buffer = new List<byte>();
    private bool fin = false;
    private Opcode opcode = Opcode.Close;

    public Frame(NetworkStream stream)
    {
      bool hasmask = false;
      byte[] mask = new byte[4];
      byte b = (byte)stream.ReadByte();
      byte b2 = (byte)(b & 0x0F);
      long length = 0;
      fin = (byte)(b & 0x80) == 0x08;
      opcode = (Opcode)b2;
      if (opcode == Opcode.Text) // Text message
      {
        b = (byte)stream.ReadByte();
        byte b1 = (byte)(b & 0x80);
        if (b1 == 0x80) // Encoded
        {
          hasmask = (byte)(b & 0x80) == 0x80;
          byte size = (byte)(b & 0x7F);
          if (size == 0x7E) // Two byte size
          {
            b1 = (byte)stream.ReadByte();
            b2 = (byte)stream.ReadByte();
          }
          else if (size == 0x7F) // Eight byte size
          {
            byte b0 = (byte)stream.ReadByte();
            b0 = (byte)stream.ReadByte();
            b0 = (byte)stream.ReadByte();
            b0 = (byte)stream.ReadByte();
            b0 = (byte)stream.ReadByte();
            b0 = (byte)stream.ReadByte();
            b0 = (byte)stream.ReadByte();
            b0 = (byte)stream.ReadByte();
          }
          else
            length = size;
          if (hasmask)
          {
            for (int i = 0; i < 4; i++)
            {
              mask[i] = (byte)stream.ReadByte();
            }
          }
          for (int i = 0; i < length; i++)
          {
            int octet = i % 4;
            b = (byte)stream.ReadByte();
            b ^= mask[octet];
            buffer.Add(b);
          }
        }
      }
    }

    public string getText()
    {
      UTF8Encoding enc = new System.Text.UTF8Encoding();
      return enc.GetString(buffer.ToArray());
    }

    public Opcode getOpcode()
    {
      return opcode;
    }
  }
}
