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
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Server
{
  /// <summary>
  /// A connected client with all it's properties (name, socket connection etc.)
  /// </summary>
  public class Client
  {
    private Thread clthread;
    private string name;
    private NetworkStream stream;
    private System.Net.WebSockets.WebSocket websocket;
    public Vehicle vehicle;
    int id;

    public enum VehicleType
    {
      Tank,
      Jeep,
      Plane,
      Bullet
    }

    public Client(int _id, string _name, Thread _thread, NetworkStream _stream, VehicleType vehicletype)
    {
      id = _id;
      clthread = _thread;
      name = _name;
      stream = _stream;
      switch (vehicletype)
      {
        case VehicleType.Tank:
          vehicle = new VehicleTank(Server.getVehicleId());
          break;
        default:
          throw new Exception("Vehicle type not implemented yet!");
      }
    }

    public override string ToString()
    {
      return name;
    }

    public Thread CLThread
    {
      get { return clthread; }
      set { clthread = value; }
    }

    public int Id
    {
      get { return id; }
      set { id = value; }
    }

    public string Name
    {
      get { return name; }
      set { name = value; }
    }

    public NetworkStream Stream
    {
      get { return stream; }
      set { stream = value; }
    }

    public System.Net.WebSockets.WebSocket WebSocket
    {
      get { return websocket; }
      set { websocket = value; }
    }
  }
}
