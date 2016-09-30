using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
  public abstract class Vehicle
  {
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
    protected bool dead = true;
    protected long deadsince = 0;
    protected bool isticking = false;
    protected bool[] functionstates = new bool[10];
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

    public abstract void Kill(); // If Vehile 'dies'
    public abstract bool isDead();
    public abstract long diedTime();
    public abstract void Wake();
  }
}
