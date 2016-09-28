using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace JustShootServer
{
  /// <summary>
  /// Summary description for Client.
  /// </summary>
  public class Client
  {
    private Thread clthread;
    private string name;
    private NetworkStream stream;
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
  }
}
