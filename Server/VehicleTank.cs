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
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Server
{
    /// <summary>
    /// The implementation of the Tank vehicle, different tanks could have different properties like movement speed or reload time. Even hitpoints (not implemented yet)
    /// </summary>
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

        private static readonly Vector3[] NormalSampleVertices = new[]
        {
            new Vector3(-40f, -20f, 50f),  // vertex 11
            new Vector3(-40f, -20f, -50f), // vertex 10
            new Vector3(40f, -20f, -50f),  // vertex 5
            new Vector3(40f, -20f, 50f)    // vertex 4
        };

        private static readonly Vector3 BarrelPivotOffset = new Vector3(0f, 25f, 14f);
        private static readonly Vector3 BarrelMuzzleOffset = new Vector3(0f, 0f, 56f);

        public VehicleTank(int tankid)
        {
            id = tankid;
            hitradius = 50;
        }

        // Do all the logic/movement based on what functions/keys are active
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
                                if (reloading <= 0.0)
                                {
                                    reloading = reloadtime;
                                    if (TryComputeBarrelPose(out var muzzle, out var dir))
                                    {
                                        var velocity = new Vector3D(dir.x, dir.y, dir.z).multiply(bulletspeed);
                                        var spawn = new Vector3D(muzzle.x, muzzle.y, muzzle.z);
                                        Bullet b = new Bullet(Server.getBulletId(), this.id, spawn, velocity);
                                        Server.bullets.Add(b);
                                    }
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
                // Clamp position to map boundaries
                double min = -(Server.landscape.scale - 100);
                double max = (Server.landscape.scale - 100);
                if (position.x < min) position.x = min;
                if (position.x > max) position.x = max;
                if (position.z < min) position.z = min;
                if (position.z > max) position.z = max;
                position.y = Server.landscape.getHeight(position.x, position.z) + 20;
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
            if (!dead)
            {
                dead = true;
                deadsince = DateTime.Now.Ticks;
            }
        }

        public override bool isDead()
        {
            return dead;
        }

        public override int deadFor()
        {
            return (int)((DateTime.Now.Ticks - deadsince) / TimeSpan.TicksPerSecond);
        }

        public override void Wake()
        {
            Random random = new Random();
            // Calculate new spawn point (not implemented yet)
            double d = random.NextDouble();
            double x = (d * 20000.0) - 10000.0;
            d = random.NextDouble();
            double z = (d * 20000.0) - 10000.0;
            set(x, 0.0, z, 0.0, 0.0, 0.0);
            // reset speed and rotations
            bodyvelocity = 0.0;
            bodyturnspeed = 0.0;
            turretturnspeed = 0.0;
            barrelturnspeed = 0.0;
            dead = false;
        }

        private bool TryComputeBarrelPose(out Vector3D muzzle, out Vector3D direction)
        {
            muzzle = null;
            direction = null;
            if (Server.landscape == null)
                return false;

            var up = ComputeLandscapeNormal();
            if (up.LengthSquared() < 1e-6f || float.IsNaN(up.X))
                up = Vector3.UnitY;

            var forward = new Vector3((float)Math.Sin(bodyrotation), 0f, (float)Math.Cos(bodyrotation));
            if (forward.LengthSquared() < 1e-6f)
                forward = Vector3.UnitZ;

            forward -= Vector3.Dot(forward, up) * up;
            if (forward.LengthSquared() < 1e-6f)
                forward = Vector3.Normalize(Vector3.Cross(Vector3.UnitX, up));
            else
                forward = Vector3.Normalize(forward);

            var right = Vector3.Cross(up, forward);
            if (right.LengthSquared() < 1e-6f)
                right = Vector3.UnitX;
            else
                right = Vector3.Normalize(right);

            forward = Vector3.Normalize(Vector3.Cross(right, up));

            var turretForward = RotateAroundAxis(forward, up, (float)turretrotation);
            var turretRight = RotateAroundAxis(right, up, (float)turretrotation);
            var turretUp = up;

            var barrelForward = RotateAroundAxis(turretForward, turretRight, (float)barrelrotation);
            var barrelUp = RotateAroundAxis(turretUp, turretRight, (float)barrelrotation);
            var barrelRight = turretRight;

            if (barrelForward.LengthSquared() < 1e-6f)
                return false;
            barrelForward = Vector3.Normalize(barrelForward);

            var bodyPosition = new Vector3((float)position.x, (float)position.y, (float)position.z);
            var pivotWorld = bodyPosition + TransformLocal(BarrelPivotOffset, turretRight, turretUp, turretForward);
            var muzzleWorld = pivotWorld + TransformLocal(BarrelMuzzleOffset, barrelRight, barrelUp, barrelForward);

            muzzle = new Vector3D(muzzleWorld.X, muzzleWorld.Y, muzzleWorld.Z);
            direction = new Vector3D(barrelForward.X, barrelForward.Y, barrelForward.Z);
            barrelmount = muzzle;
            barreldirection = direction;
            return true;
        }

        private Vector3 ComputeLandscapeNormal()
        {
            var points = new Vector3[NormalSampleVertices.Length];
            double sin = Math.Sin(bodyrotation);
            double cos = Math.Cos(bodyrotation);

            for (int i = 0; i < NormalSampleVertices.Length; i++)
            {
                var vertex = NormalSampleVertices[i];
                double localForward = vertex.Z;
                double localSide = vertex.X;
                double worldX = position.x + sin * localForward + cos * localSide;
                double worldZ = position.z + cos * localForward - sin * localSide;
                double height = Server.landscape.getHeight(worldX, worldZ);
                points[i] = new Vector3((float)worldX, (float)height, (float)worldZ);
            }

            var p1 = points[1];
            var p2 = points[2];
            var p3 = points[3];

            var normal = new Vector3(
                (p1.Y - p2.Y) * (p1.Z - p3.Z) - (p1.Z - p2.Z) * (p1.Y - p3.Y),
                -((p1.Z - p2.Z) * (p1.X - p3.X) - (p1.X - p2.X) * (p1.Z - p3.Z)),
                (p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X)
            );

            if (normal.LengthSquared() < 1e-6f || float.IsNaN(normal.X))
                return Vector3.UnitY;

            normal = Vector3.Normalize(normal);
            normal.X = -normal.X;
            normal.Z = -normal.Z;
            return normal;
        }

        private static Vector3 RotateAroundAxis(Vector3 vector, Vector3 axis, float angle)
        {
            if (Math.Abs(angle) < 1e-6f)
                return vector;
            axis = Vector3.Normalize(axis);
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return vector * cos + Vector3.Cross(axis, vector) * sin + axis * Vector3.Dot(axis, vector) * (1 - cos);
        }

        private static Vector3 TransformLocal(Vector3 local, Vector3 right, Vector3 up, Vector3 forward)
        {
            return right * local.X + up * local.Y + forward * local.Z;
        }

    }
}
