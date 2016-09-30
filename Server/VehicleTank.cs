using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Server
{
  class VehicleTank : Vehicle
  {
    public double bodyrotation = 0;
    public double turretrotation = 0;
    public double barrelrotation = 0;
    double reloadtime = 10.0; // seconds
    double reloading = 0.0;
    double bodyvelocity = 0.0;
    double bodymaxvelocity = 3.0;
    double bodyacceleration = 0.05;
    double bodydeceleration = 0.1;
    double bodyturnspeed = 0.0;
    double bodyturnacceleration = 0.001;
    double bodyturndeceleration = 0.003;
    double bodymaxturnspeed = 0.01;

    double turretturnspeed = 0.0;
    double turretturnacceleration = 0.001;
    double turretturndeceleration = 0.003;
    double turretmaxturnspeed = 0.01;

    double barrelturnspeed = 0.0;
    double barrelturnacceleration = 0.001;
    double barrelturndeceleration = 0.003;
    double barrelmaxturnspeed = 0.005;
    double barrelminturn = 0.0;
    double barrelmaxturn = 0.3;
    double bulletspeed = 20.0;

    public Vector3D barrelmount;
    public Vector3D barreldirection;

    public VehicleTank(int tankid)
    {
      id = tankid;
      hitradius = 50;
    }
    
    public override void Tick()
    {
      if (!isticking)
      {
        isticking = true;

        // Adjust acording to keys pressed
        for (int i = 1; i < 10; i++)
        {
          VehicleFunction f = (VehicleFunction)i;
          switch (f)
          {
            case VehicleFunction.PrimaryLeft: // Left cursor key
              if (functionstates[i])
              {
                bodyturnspeed -= bodyturnacceleration;
                if (bodyturnspeed < -bodymaxturnspeed)
                  bodyturnspeed = -bodymaxturnspeed;
              }
              else  // If we are not turning the tank left it should stop turning
              {
                if (bodyturnspeed < 0)
                {
                  bodyturnspeed += bodyturndeceleration;
                  if (bodyturnspeed > 0)
                    bodyturnspeed = 0;
                }
              }
              break;
            case VehicleFunction.PrimaryRight: // Right cursor key
              if (functionstates[i])
              {
                bodyturnspeed += bodyturnacceleration;
                if (bodyturnspeed > bodymaxturnspeed)
                  bodyturnspeed = bodymaxturnspeed;
              }
              else  // If we are not turning the tank left it should stop turning
              {
                if (bodyturnspeed > 0)
                {
                  bodyturnspeed -= bodyturndeceleration;
                  if (bodyturnspeed < 0)
                    bodyturnspeed = 0;
                }
              }
              break;
            case VehicleFunction.PrimaryUp: // Up cursor key
              if (functionstates[i])
              {
                bodyvelocity += bodyacceleration;
                if (bodyvelocity > bodymaxvelocity)
                  bodyvelocity = bodymaxvelocity;
              }
              else  // If we are not accelerating the tank it should slowley stop
              {
                if (bodyvelocity > 0)
                {
                  bodyvelocity -= bodydeceleration;
                  if (bodyvelocity < 0)
                    bodyvelocity = 0;
                }
              }
              break;
            case VehicleFunction.PrimaryDown: // Down cursor key
              if (functionstates[i])
              {
                bodyvelocity -= bodyacceleration;
                if (bodyvelocity < -bodymaxvelocity)
                  bodyvelocity = -bodymaxvelocity;
              }
              else  // If we are not reversing the tank it should slowley stop reversing
              {
                if (bodyvelocity < 0)
                {
                  bodyvelocity += bodydeceleration;
                  if (bodyvelocity > 0)
                    bodyvelocity = 0;
                }
              }
              break;
            case VehicleFunction.SecondaryLeft: // A key, turn turet left
              if (functionstates[i])
              {
                turretturnspeed -= turretturnacceleration;
                if (turretturnspeed < -turretmaxturnspeed)
                  turretturnspeed = -turretmaxturnspeed;
              }
              else  // If we are not turning the turret left it should stop turning
              {
                if (turretturnspeed < 0.0)
                {
                  turretturnspeed += turretturndeceleration;
                  if (turretturnspeed > 0.0)
                    turretturnspeed = 0.0;
                }
              }
              break;
            case VehicleFunction.SecondaryRight: // D key, turn turet right
              if (functionstates[i])
              {
                turretturnspeed += turretturnacceleration;
                if (turretturnspeed > turretmaxturnspeed)
                  turretturnspeed = turretmaxturnspeed;
              }
              else  // If we are not turning the turret left it should stop turning
              {
                if (turretturnspeed > 0.0)
                {
                  turretturnspeed -= turretturndeceleration;
                  if (turretturnspeed < 0.0)
                    turretturnspeed = 0.0;
                }
              }
              break;
            case VehicleFunction.SecondaryUp: // W key, raise barrel
              if (functionstates[i])
              {
                barrelturnspeed -= barrelturnacceleration;
                if (barrelturnspeed < -barrelmaxturnspeed)
                  barrelturnspeed = -barrelmaxturnspeed;
              }
              else  // If we are not turning the barrel left it should stop turning
              {
                if (barrelturnspeed < 0.0)
                {
                  barrelturnspeed += barrelturndeceleration;
                  if (barrelturnspeed > 0.0)
                    barrelturnspeed = 0.0;
                }
              }
              break;
            case VehicleFunction.SecondaryDown: // S key, lower barrel
              if (functionstates[i])
              {
                barrelturnspeed += barrelturnacceleration;
                if (barrelturnspeed > barrelmaxturnspeed)
                  barrelturnspeed = barrelmaxturnspeed;
              }
              else  // If we are not turning the barrel left it should stop turning
              {
                if (barrelturnspeed > 0.0)
                {
                  barrelturnspeed -= barrelturndeceleration;
                  if (barrelturnspeed < 0.0)
                    barrelturnspeed = 0.0;
                }
              }
              break;
            case VehicleFunction.Fire:
              if (functionstates[i])
              {
                if (reloading == 0.0)
                {
                  reloading = reloadtime;
                  Bullet b = new Bullet(Server.getBulletId(), this.id, barrelmount, barreldirection.multiply(bulletspeed));
                  Server.bullets.Add(b);
                }
              }
                break;
          }
        }
        double delta = 3.00;

        if (reloading > 0)
        {
          reloading -= 0.2;
          if (reloading < 0.0)
            reloading = 0.0;
        }

        if (barrelturnspeed < 0 && barrelrotation > -barrelmaxturn)
          barrelrotation += barrelturnspeed * delta;
        if (barrelturnspeed > 0 && barrelrotation <= -barrelminturn)
          barrelrotation += barrelturnspeed * delta;

        bodyrotation -= bodyturnspeed * delta;
        turretrotation -= turretturnspeed * delta;
        var movex = Math.Sin(bodyrotation) * bodyvelocity * delta;
        var movez = Math.Cos(bodyrotation) * bodyvelocity * delta;
        position.x += movex;
        position.z += movez;
        position.y = Server.landscape.getHeight(position.x, position.z)+20;
        if (position.x < -(Server.landscape.scale - 100)) position.x = -(Server.landscape.scale - 100);
        if (position.x > (Server.landscape.scale - 100)) position.x = (Server.landscape.scale - 100);
        if (position.z < -(Server.landscape.scale - 100)) position.x = -(Server.landscape.scale - 100);
        if (position.z > (Server.landscape.scale - 100)) position.z = (Server.landscape.scale - 100);
      }
      isticking = false;
    }

    public void set(double x, double y, double z, double r, double tr, double br)
    {
      position.x = x;
      position.y = y;
      position.z = z;
      bodyrotation = r;
      turretrotation = tr;
      barrelrotation = br;
    }

    public override string toJSON()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("{\"p\":\"");
      sb.Append(id);
      sb.Append(";");
      sb.Append((int)position.x);
      sb.Append(";");
/*      sb.Append((int)position.y);
      sb.Append(";");*/
      sb.Append((int)position.z);
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", bodyrotation));
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", turretrotation));
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", barrelrotation));
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", bodyvelocity));
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", bodyturnspeed));
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", turretturnspeed));
      sb.Append(";");
      sb.Append(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", barrelturnspeed));
      sb.Append("\"}");
      return sb.ToString();
    }

    public override void Kill()
    {
      dead = true;
      deadsince = DateTime.Now.Ticks;
    }

    public override bool isDead()
    {
      return dead;
    }

    public override long diedTime()
    {
      return deadsince;
    }

    public override void Wake()
    {
      Random random = new Random();
      double d = random.NextDouble();
      double x = (d*20000.0)-10000.0;
      d = random.NextDouble();
      double z = (d*20000.0)-10000.0;
      set(x, 0.0, z, 0.0, 0.0, 0.0);
      dead = false;
    }

  }
}
