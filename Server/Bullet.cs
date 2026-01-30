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
using System.Globalization;

namespace Server
{
  /// <summary>
  /// This class holds the properties for bullets fiered in the game. Bullets are affected by gravity and air. Bullets can make contact with either another tank or the ground.
  /// </summary>
  class Bullet
  {
    public int id;
    public int vehicleid; // Who shot the bullet
    public Vector3D position;
    public Vector3D velocity;
    private double gravity = 0.00982;
    private double air = 0.99; // Reduction in speed due to air drag.
    private bool hitground = false;
    private bool hittank = false;

    public Bullet(int bid, int vid, Vector3D pos, Vector3D vel)
    {
      id = bid;
      vehicleid = vid;
      position = pos;
      velocity = vel;
    }

    // The tick is run at a fixed delay/interval and detects if the bullet hit the ground.
    public void Tick()
    {
      if (!hitground && !hittank)
      {
        position.x += velocity.x * 2;
        position.y += velocity.y * 2;
        position.z += velocity.z * 2;
        velocity.x *= air;
        velocity.y -= gravity;
        velocity.z *= air;
        if (position.y < Landscape.landscape.getHeight(position.x, position.z))
        {
          //Server.Log("Bullet hit ground", ConsoleColor.Red);
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

    /// <summary>
    /// Generates the JSON with the bullets properties.
    /// </summary>
    /// <returns>JSON used by the javascript client</returns>
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
