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

namespace Server
{
  // A simple 3D vector class. Might be expanded in the future when server takes over from the client (to prevent cheating)
  public class Vector3D
  {
    public double x;
    public double y;
    public double z;

    public Vector3D(double _x, double _y, double _z)
    {
      x = _x;
      y = _y;
      z = _z;
    }

    public Vector3D multiply(double factor)
    {
      x *= factor;
      y *= factor;
      z *= factor;
      return this;
    }
  }
}
