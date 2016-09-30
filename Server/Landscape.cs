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
  /// Simple landscape generator. Ported from Turbo Pascal code I wrote a long time ago for a similar DOS based Scorched Earth like 3D game.
  /// </summary>
 
  class Landscape
  {
    StringBuilder json = new StringBuilder();
    public double scale = 20000;
    public double[,] data;
    public int landscapesize;
    public static Landscape landscape;

    public Landscape(int size)
    {
      landscape = this;
      landscapesize = size - 1;
      Random random = new Random();
      //size of grid to generate, note this must be a
      //value 2^n+1
      double h = 2500.0; //the range (-h -> +h) for the average offset
      double SEED = 500.0; //an initial seed value for the corners of the data
      data = new double[size, size];
      //seed the data

      // Fill it with 0
      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          data[x, y] = 0;
        }
      }

      data[0, 0] = random.NextDouble() * (2 * SEED) - SEED;
      data[0, size - 1] = random.NextDouble() * (2 * SEED) - SEED;
      data[size - 1, 0] = random.NextDouble() * (2 * SEED) - SEED;
      data[size - 1, size - 1] = random.NextDouble() * (2 * SEED) - SEED;

      //side length is distance of a single square side
      //or distance of diagonal in diamond
      for (int sideLength = size - 1;
        //side length must be >= 2 so we always have
        //a new value (if its 1 we overwrite existing values
        //on the last iteration)
          sideLength >= 2;
        //each iteration we are looking at smaller squares
        //diamonds, and we decrease the variation of the offset
          sideLength /= 2, h /= 2.0)
      {
        //half the length of the side of a square
        //or distance from diamond center to one corner
        //(just to make calcs below a little clearer)
        int halfSide = sideLength / 2;

        //generate the new square values
        for (int x = 0; x < size - 1; x += sideLength)
        {
          for (int y = 0; y < size - 1; y += sideLength)
          {
            //x, y is upper left corner of square
            //calculate average of existing corners
            double avg = data[x, y] + //top left
            data[x + sideLength, y] + //top right
            data[x, y + sideLength] + //lower left
            data[x + sideLength, y + sideLength]; //lower right
            avg /= 4.0;

            //center is average plus random offset
            data[x + halfSide, y + halfSide] =
              //We calculate random value in range of 2h
              //and then subtract h so the end value is
              //in the range (-h, +h)
          avg + (random.NextDouble() * 2 * h) - h;
          }
        }

        //generate the diamond values
        //since the diamonds are staggered we only move x
        //by half side
        //NOTE: if the data shouldn't wrap then x < DATA_SIZE
        //to generate the far edge values
        for (int x = 0; x < size - 1; x += halfSide)
        {
          //and y is x offset by half a side, but moved by
          //the full side length
          //NOTE: if the data shouldn't wrap then y < DATA_SIZE
          //to generate the far edge values
          for (int y = (x + halfSide) % sideLength; y < size - 1; y += sideLength)
          {
            //x, y is center of diamond
            //note we must use mod  and add DATA_SIZE for subtraction 
            //so that we can wrap around the array to find the corners
            double avg =
              data[(x - halfSide + size) % size, y] + //left of center
              data[(x + halfSide) % size, y] + //right of center
              data[x, (y + halfSide) % size] + //below center
              data[x, (y - halfSide + size) % size]; //above center
            avg /= 4.0;

            //new value = average plus random offset
            //We calculate random value in range of 2h
            //and then subtract h so the end value is
            //in the range (-h, +h)
            avg = avg + (random.NextDouble() * 2 * h) - h;
            //update value for center of diamond
            data[x, y] = avg;

            //wrap values on the edges, remove
            //this and adjust loop condition above
            //for non-wrapping values.
            if (x == 0) data[size - 1, y] = avg;
            if (y == 0) data[x, size - 1] = avg;
          }
        }
      }
      json.Append("[");
      for (int x = 0; x < size; x++)
      {
        if (x != 0)
          json.Append(",");
        json.Append("[");
        for (int y = 0; y < size; y++)
        {
          //data[x, y] = Math.Sin((double)y/5.0)*2000.0;
          if (y != 0)
            json.Append(",");
          json.Append(data[x, y].ToString(CultureInfo.InvariantCulture));
        }
        json.Append("]");
      }
      json.Append("]");
    }

    public string toJSON()
    {
      return json.ToString();
    }

    private double getHeight(double px, double pz, int x1, int z1, int x2, int z2, int x3, int z3)
    {
      if (x1 > 256 || z1 > 256 || x2 > 256 || z2 > 256 || x3 > 256 || z3 > 256 || x1<0 ||x2<0||x3<0||z1<0||z2<0||z3<0)
        return 0;
      double h1 = data[x1, z1];
      double h2 = data[x2, z2];
      double h3 = data[x3, z3];
      double diffx = h1 - h2;
      double diffz = h3 - h2;
      double h = px * diffx + pz * diffz;
      return h2 + h;
    }

    private double pointHeight(double _x, double _z)
    {
      double h = 0;
      int x = (int)Math.Floor(_x);
      int z = (int)Math.Floor(_z);
      double tx = _x - x;
      double tz = _z - z;
      if (tx + tz > 1)
      { // top right triangle
        h = getHeight(1 - Math.Abs(_x - x), 1 - Math.Abs(_z - z), x, z + 1, x + 1, z + 1, x + 1, z);
      }
      else
      { // bottom left triangle
        h = getHeight(Math.Abs(_x - x), Math.Abs(_z - z), x + 1, z, x, z, x, z + 1);
      }
      return h;
    }

    private double toHeightMapX(double x)
    {
      x = x + scale;
      x = x / ((scale * 2) / landscapesize);
      return x;
    }

    private double toHeightMapZ(double z)
    {
      z = z + scale;
      z = z / ((scale * 2) / landscapesize);
      return z;
    }

    public double getHeight(double x, double z)
    {
      double px = toHeightMapX(x);
      double pz = toHeightMapZ(z);
      return pointHeight(px, pz);
    }


  }
}
