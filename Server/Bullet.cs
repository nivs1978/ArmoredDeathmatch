using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Server
{
  class Bullet
  {
    public int id;
    public int vehicleid;
    public Vector3D position;
    public Vector3D velocity;
    private double gravity = 0.00982;
    private double wind = 0.99;
    private bool hitground = false;
    private bool hittank = false;

    public Bullet(int bid, int vid, Vector3D pos, Vector3D vel)
    {
      id = bid;
      vehicleid = vid;
      position = pos;
      velocity = vel;
    }

    public void Tick()
    {
      if (!hitground && !hittank)
      {
        position.x += velocity.x * 2;
        position.y += velocity.y * 2;
        position.z += velocity.z * 2;
        velocity.x *= wind;
        velocity.y -= gravity;
        velocity.z *= wind;
        if (position.y < Landscape.landscape.getHeight(position.x, position.z))
        {
          Server.Log("Bullet hit ground", ConsoleColor.Red);
          hitground = true;
        }
      }
    }

    public bool hitGround()
    {
      return hitground;
    }

    public bool hitVehicle(Vehicle v)
    {
      double x = v.position.x-this.position.x;
      double y = v.position.y-this.position.y;
      double z = v.position.z-this.position.z;
      return Math.Sqrt(x*x+y*y+z*z)<v.hitradius;
    }

    public string toJSON()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("{\"b\":\"");
      sb.Append(id);
      sb.Append(";");
      sb.Append(vehicleid);
      sb.Append(";");
      sb.Append((int)position.x);
      sb.Append(";");
      sb.Append((int)position.y);
      sb.Append(";");
      sb.Append((int)position.z);
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", velocity.x));
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", velocity.y));
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", velocity.z));
      sb.Append("\"}");
      return sb.ToString();
    }
  }
}
