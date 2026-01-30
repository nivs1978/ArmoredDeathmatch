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
  /// <summary>
  /// A vehicle is a user controlled object in the game. This is for future expansion with other vehicle types than tanks (eg cars, boats, planes etc.)
  /// </summary>
  public abstract class Vehicle
  {
    // Primary keys are like forward, backwards, left right (sually the arrow keys), secondary are WASD keys (For the tank it moves the turret and barrel). Space usually fires whatever weapon the vehicle has.
    public enum VehicleFunction {
      PrimaryLeft = 1,
      PrimaryRight = 2,
      PrimaryUp = 3,
      PrimaryDown = 4,
      SecondaryLeft = 5,
      SecondaryRight = 6,
      SecondaryUp = 7,
      SecondaryDown = 8,
      Fire = 9
    }
    protected bool dead = true; // If the vehicle has been killed/disabled
    protected long deadsince = 0; // For how long has the vehicle been dead (used for respawn after a certain time)
    protected bool isticking = false; // Is the vehicle beeing updated by the timers
    protected bool[] functionstates = new bool[10]; // What functions or keypresses are currently active
    public int id;
    public Vector3D position = new Vector3D(0, 0, 0);
    public double hitradius = 10;

    public void functionPressed(VehicleFunction function) // When a key is pressed it is stored in the vehcile and the Tick function then manipulates the vehicle
    {
      functionstates[(int)function] = true;
    }

    public void functionReleased(VehicleFunction function) // When a key is released it is stored and the Tick function stops manipulating the vehicle 
    {
      functionstates[(int)function] = false;
    }

    public bool isFunctionPressed(VehicleFunction function)
    {
      return functionstates[(int)function];
    }

    public abstract void Tick(); // Loops trough the manipulator functions and manipulate the vehicle

    public abstract string toJSON(); // Returns the vehicles position, rotation and other parameters as json

    public abstract void Kill(); // If Vehile 'dies' this is to be called
    public abstract bool isDead();
    public abstract int deadFor();
    public abstract void Wake();
  }
}
