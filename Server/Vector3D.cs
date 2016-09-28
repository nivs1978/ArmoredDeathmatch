using System;
using System.Collections.Generic;
using System.Text;

namespace JustShootServer
{
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
