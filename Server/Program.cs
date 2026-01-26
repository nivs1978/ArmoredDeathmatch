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

using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    /// <summary>
    /// This is the main server class that handles all the game logic and clients connected.
    /// </summary>
    class Server
    {
        public static ArrayList clients;
        public static Landscape landscape;
        private static Timer engineticker;
        private static Timer opponentticker;
        private static Timer positionticker;
        public static List<Bullet> bullets = new List<Bullet>();
        private static bool positionticking = false;
        private static bool bulletarraylocked = false;
        private static readonly object _syncObject = new object();
        private static readonly object clientsLock = new object();
        private static readonly object bulletsLock = new object();
        private static bool _initialized = false;

        public static void Log(string text, ConsoleColor c)
        {
            lock (_syncObject)
            {
                Console.ForegroundColor = c;
                Console.WriteLine(text);
                using (StreamWriter sw = File.AppendText("ADServer.log"))
                {
                    sw.WriteLine(text);
                }
            }
        }

        // Send text to client. Either messages or updated positions of objects in the world.
        private static bool sendText(NetworkStream stream, string text)
        {
            try
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
                List<byte> buffer = new List<byte>();
                byte b1 = (byte)((byte)0x80 | ((byte)Frame.Opcode.Text & 0x0f));
                buffer.Add(b1);
                if (data.Length <= 125)
                {
                    buffer.Add((byte)(data.Length));
                }
                else if (data.Length >= 126 && data.Length <= 65535)
                {
                    buffer.Add(0x7E);
                    buffer.Add((byte)((data.Length >> 8) & 255));
                    buffer.Add((byte)((data.Length) & 255));
                }
                else
                {
                    buffer.Add(0x7F);
                    byte[] b = BitConverter.GetBytes((Int64)data.Length);
                    buffer.Add(b[7]);
                    buffer.Add(b[6]);
                    buffer.Add(b[5]);
                    buffer.Add(b[4]);
                    buffer.Add(b[3]);
                    buffer.Add(b[2]);
                    buffer.Add(b[1]);
                    buffer.Add(b[0]);
                }
                for (int i = 0; i < data.Length; i++)
                    buffer.Add((byte)(data[i]/* ^ 0*/));
                byte[] tosend = buffer.ToArray();
                if (stream.CanWrite)
                {
                    stream.Write(tosend, 0, tosend.Length);
                    stream.Flush();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), ConsoleColor.Red);
            }
            return false;
        }

        private static void sendText(WebSocket ws, string text)
        {
            try
            {
                var buffer = Encoding.UTF8.GetBytes(text);
                var seg = new ArraySegment<byte>(buffer);
                ws.SendAsync(seg, WebSocketMessageType.Text, true, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), ConsoleColor.Red);
            }
        }

        private static void sendText(Client c, string text)
        {
            if (c == null) return;
            if (c.Stream != null)
                sendText(c.Stream, text);
            else if (c.WebSocket != null)
                sendText(c.WebSocket, text);
        }
        // Handle a client connected over WebSocket (used by ASP.NET Core endpoint)
        public static async Task HandleWebSocket(WebSocket webSocket)
        {
            int id = getVehicleId();
            Client client = new Client(id, null, null, null, Client.VehicleType.Tank);
            client.WebSocket = webSocket;
            // Ensure server state is initialized if background Start hasn't run yet
            if (clients == null) clients = new ArrayList();
            if (landscape == null) landscape = new Landscape(257);

            lock (clientsLock)
            {
                clients.Add(client);
            }
            // Send id and initial landscape + client list to the new client
            sendText(client, "{ \"type\": \"id\", \"id\": " + client.Id + ", \"name\": null}");
            sendText(client, "{ \"type\": \"chat\", \"id\": -1, \"text\": \"Receiving landscape\"}");
            sendText(client, "{ \"type\": \"landscape\", \"data\": " + landscape.toJSON() + "}");
            sendText(client, "{ \"type\": \"chat\", \"id\": -1, \"text\": \"Receiving client list\"}");
            await Task.Delay(50);
            sendText(client, GetClientListJSON());

            var buffer = new byte[4096];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        if (!string.IsNullOrEmpty(message))
                            processMessage(client, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), ConsoleColor.Red);
            }

            Client remove = null;
            lock (clientsLock)
            {
                foreach (Client c in clients)
                {
                    if (c.WebSocket == webSocket)
                    {
                        remove = c;
                        break;
                    }
                }
            }
            if (remove != null)
            {
                try { remove.WebSocket?.Abort(); } catch { }
                lock (clientsLock)
                {
                    clients.Remove(remove);
                }
                sendToAllClients(remove, "{ \"type\": \"quit\", \"id\": \"" + remove.Id + "\"}");
                Log("* " + remove.Name + " quit", ConsoleColor.Blue);
            }
        }

        public static int getVehicleId()
        {
            int available = 0;
            lock (clientsLock)
            {
                foreach (Client cl in clients)
                {
                    available = Math.Max(cl.Id, available);
                }
            }
            available++;
            return available;
        }

        public static int getBulletId()
        {
            int available = 0;
            foreach (Bullet b in bullets)
            {
                available = Math.Max(b.id, available);
            }
            available++;
            return available;
        }

        public static void sendToAllClients(Client c, string json)
        {
            List<Client> remove = new List<Client>();
            for (int n = 0; n < clients.Count; n++)
            {
                Client cl = (Client)clients[n];
                if ((c == null || cl.Id != c.Id) && (cl.Stream != null || cl.WebSocket != null)) // Do not send message to self or one that has no name
                {
                    try
                    {
                        sendText(cl, json);
                    }
                    catch (Exception ex)
                    {
                        remove.Add(cl); // Something went wrong with the connection. Flag client to be removed
                        Log(ex.ToString(), ConsoleColor.Red);
                    }
                }
            }
            foreach (Client cl in remove)
            {
                clients.Remove(cl);
                Log("* " + cl.Name + " quit", ConsoleColor.Blue);
            }
            foreach (Client cl in remove)
            {
                sendToAllClients(null, "{ \"type\": \"quit\", \"id\": \"" + cl.Id + "\"}");
            }
        }

        private static void processMessage(Client client, string message)
        {
            if (message.ToLower().StartsWith("/"))
            {
                if (message.ToLower().StartsWith("/nick"))
                {
                    string name = message.Substring(6);
                    bool nametaken = false;
                    lock (clientsLock)
                    {
                        foreach (Client c in clients)
                        {
                            if (client.Name != null && c.Name.ToLower().CompareTo(name.ToLower()) == 0)
                            {
                                nametaken = true;
                                break;
                            }
                        }
                    }
                    if (nametaken)
                    {
                        sendText(client, "{ \"type\": \"error\", \"text\": \"Nickname already in use\"}");
                        Log("* Name " + name + " taken", ConsoleColor.DarkYellow);
                    }
                    else
                    {
                        if (client.Name != null) // person already joined, nickchange
                        {
                            Log("* " + client.Name + " change name to " + name, ConsoleColor.DarkGreen);
                            sendToAllClients(null, "{ \"type\": \"nick\", \"id\": \"" + client.Id + "\", \"name\": \"" + name + "\"}");
                            client.Name = name;
                        }
                        else // New client joined
                        {
                            client.Name = name;
                            Log("* " + name + "(" + client.Id + ") joined", ConsoleColor.DarkGreen);
                            sendText(client, "{ \"type\": \"chat\", \"id\": -1, \"text\": \"Name accepted\"}");
                            sendText(client, "{ \"type\": \"id\", \"id\": " + client.Id + ", \"name\": \"" + name + "\", \"score\": " + client.Score + "}");
                            sendToAllClients(client, "{ \"type\": \"join\", \"id\": " + client.Id + ", \"name\": \"" + name + "\", \"score\": " + client.Score + "}");
                            client.vehicle.Wake();
                        }
                    }
                }
                else
                {
                    sendText(client, "{ \"type\": \"error\", \"text\": \"Unknown command: " + message + "\"}");
                    Log("* error: " + message, ConsoleColor.Red);
                }
            }
            else if (message.ToLower().StartsWith("+")) // Key down
            {
                if (message.IndexOf(';') == -1)
                    client.vehicle.functionPressed((Vehicle.VehicleFunction)Int32.Parse(message.Substring(1)));
                else
                {
                    string[] args = message.Split(';');
                    if (client.vehicle is VehicleTank)
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            string str = args[i];
                            if (str.Length > 5)
                                str = str.Substring(0, 5);
                            Console.Write(str + "\t");
                        }
                        Console.WriteLine();
                        ((VehicleTank)client.vehicle).barrelmount = new Vector3D(Double.Parse(args[1], CultureInfo.InvariantCulture), Double.Parse(args[2], CultureInfo.InvariantCulture), Double.Parse(args[3], CultureInfo.InvariantCulture));
                        ((VehicleTank)client.vehicle).barreldirection = new Vector3D(Double.Parse(args[4], CultureInfo.InvariantCulture), Double.Parse(args[5], CultureInfo.InvariantCulture), Double.Parse(args[6], CultureInfo.InvariantCulture));
                    }
                    client.vehicle.functionPressed((Vehicle.VehicleFunction)Int32.Parse(args[0].Substring(1)));
                }
            }
            else if (message.ToLower().StartsWith("-")) // Key up
            {
                client.vehicle.functionReleased((Vehicle.VehicleFunction)Int32.Parse(message.Substring(1)));
            }
        }
        /*

        private static void SendToClient(Client cl, string message)
        {
            try
            {
                sendText(cl, message);
            }
            catch
            {
                try { cl.Stream?.Close(); } catch { }
                try { cl.WebSocket?.Abort(); } catch { }
                try { cl.CLThread?.Abort(); } catch { }
                clients.Remove(cl);
                sendToAllClients(null, "{ \"type\": \"quit\", \"id\": \"" + cl.Id + "\"}");
                Log("* " + cl.Name + " quit", ConsoleColor.Blue);
            }
        }
        */
        private static string GetClientListJSON()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append("{\"type\": \"clients\", \"list\": [");
            bool first = true;
            for (int n = 0; n < clients.Count; n++)
            {
                Client cl = (Client)clients[n];
                if (cl.Name != null)
                {
                    if (!first)
                        ret.Append(",");
                    first = false;
                    ret.Append("{ \"id\": " + cl.Id + ", \"name\": \"" + cl.Name + "\", \"score\": " + cl.Score + " }");
                }
            }
            ret.Append("]}");
            return ret.ToString();
        }

        private static Random r = new Random();

        // For now the AI is doing "monkey business" by pressing and releasing random keys
        private static void doAI(Vehicle v)
        {
            r.Next(1, 20);
            if (r.Next(0, 9) == 0)
            {
                int key = r.Next(1, 9);
                if (v.isFunctionPressed((Vehicle.VehicleFunction)key))
                    v.functionReleased((Vehicle.VehicleFunction)key);
                else
                    v.functionPressed((Vehicle.VehicleFunction)key);
            }
        }

        private static bool engineticking = false;

        private static void EngineTick(object state)
        {
            if (!engineticking)
            {
                engineticking = true;
                lock (clientsLock)
                {
                    foreach (Client c in clients)
                    {
                        if (!c.vehicle.isDead())
                            c.vehicle.Tick();
                        else
                        {
                            long now = DateTime.Now.Ticks;
                            now = now - c.vehicle.diedTime();
                            now = now / TimeSpan.TicksPerSecond;
                            if (now > 10)
                            {

                            }
                        }
                        if (c.Stream != null || c.WebSocket != null)
                        {
                            /*VehicleTank vt = (VehicleTank)autos[0];
                            VehicleTank v = (VehicleTank)c.vehicle;
                            vt.set(v.position.x, v.position.y, v.position.z, v.bodyrotation, v.turretrotation, v.barrelrotation);*/
                        }
                        else
                        {
                            doAI(c.vehicle);
                        }
                    }
                }
                if (!bulletarraylocked)
                {
                    List<int> remove = new List<int>();
                    List<string> broadcasts = new List<string>();
                    bool clientListDirty = false;
                    // Process bullets under bulletsLock to avoid concurrent modification
                    lock (bulletsLock)
                    {
                        for (int i = 0; i < bullets.Count; i++)
                        {
                            bullets[i].Tick();
                            if (bullets[i].hitGround())
                            {
                                broadcasts.Add("{ \"type\": \"bhit\", \"id\": " + bullets[i].id + ", \"x\": " + bullets[i].position.x.ToString(CultureInfo.InvariantCulture) + ", \"z\": " + bullets[i].position.z.ToString(CultureInfo.InvariantCulture) + "}");
                                remove.Add(i);
                                continue;
                            }
                            bool hitDetected = false;
                            Client hitClient = null;
                            lock (clientsLock)
                            {
                                foreach (Client c in clients)
                                {
                                    if (bullets[i].hitVehicle(c.vehicle))
                                    {
                                        // Mark hit and perform state changes under clientsLock
                                        hitDetected = true;
                                        hitClient = c;
                                        c.vehicle.Kill();
                                        int shooterVehicleId = bullets[i].vehicleid;
                                        foreach (Client shooter in clients)
                                        {
                                            if (shooter.vehicle != null && shooter.vehicle.id == shooterVehicleId)
                                            {
                                                shooter.Score += 1;
                                                clientListDirty = true;
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            if (hitDetected && hitClient != null)
                            {
                                broadcasts.Add("{ \"type\": \"vhit\", \"bid\": " + bullets[i].id + ", \"vid\": " + hitClient.vehicle.id + ", \"x\": " + bullets[i].position.x.ToString(CultureInfo.InvariantCulture) + ", \"z\": " + bullets[i].position.z.ToString(CultureInfo.InvariantCulture) + " }");
                                remove.Add(i);
                            }
                        }

                        for (int idx = remove.Count - 1; idx >= 0; idx--)
                        {
                            int ri = remove[idx];
                            if (bullets.Count > ri)
                                bullets.RemoveAt(ri);
                        }
                    }

                    // Send notifications after releasing locks to avoid blocking while holding locks
                    foreach (var msg in broadcasts)
                    {
                        sendToAllClients(null, msg);
                    }
                    if (clientListDirty)
                    {
                        sendToAllClients(null, GetClientListJSON());
                    }
                }
        }
        engineticking = false;
        }

        private static void OpponentTick(object state)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            if (!positionticking)
            {
                positionticking = true;
                lock (clientsLock)
                {
                    foreach (Client c in clients)
                    {
                        if (c.Name != null)
                            sendToAllClients(c, c.vehicle.toJSON());
                    }
                }
                bulletarraylocked = true;
                lock (bulletsLock)
                {
                    foreach (Bullet b in bullets)
                    {
                        sendToAllClients(null, b.toJSON());
                    }
                }
                bulletarraylocked = false;
                positionticking = false;
            }
            //else
            //throw new Exception("Tick before last tick complete");
            s.Stop();
            //      Log("PosTick took " + s.ElapsedMilliseconds + " ms", ConsoleColor.White);
        }


        private static void PositionTick(object state)
        {
            lock (clientsLock)
            {
                foreach (Client c in clients)
                {
                    if (c.Stream != null || c.WebSocket != null)
                        sendText(c, c.vehicle.toJSON());
                }
            }
        }

        public static void Start(System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                EnsureInitialized();
                // EnsureInitialized already called; do not recreate landscape/clients here
                // No raw TcpListener thread; WebSocket connections are accepted via ASP.NET /chat endpoint.

                for (int i = 1; i < 5; i++)
                {
                    int id = getVehicleId();
                    Client c = new Client(id, "CPU" + i, System.Threading.Thread.CurrentThread, null, Client.VehicleType.Tank);
                    c.vehicle.Wake();
                    clients.Add(c);
                }

                engineticker = new System.Threading.Timer(EngineTick, null, 0, 20);
                opponentticker = new System.Threading.Timer(OpponentTick, null, 0, 100);
                positionticker = new System.Threading.Timer(PositionTick, null, 0, 25);

#if DEBUG
                try { Process.Start("http://localhost:8005/"); } catch { }
#endif

                // Wait until cancellation is requested
                while (!cancellationToken.IsCancellationRequested)
                {
                    System.Threading.Thread.Sleep(200);
                }

                // Begin shutdown
                try { engineticker?.Dispose(); opponentticker?.Dispose(); positionticker?.Dispose(); } catch { }
            }
            catch (Exception ex)
            {
                Log("Fejl: " + ex, ConsoleColor.Red);
            }
        }

        static string ReadLine(Stream stream)
        {
            var sb = new StringBuilder();
            var buffer = new List<byte>();
            while (true)
            {
                buffer.Add((byte)stream.ReadByte());
                var line = Encoding.ASCII.GetString(buffer.ToArray());
                if (line.EndsWith(Environment.NewLine))
                {
                    return line.Substring(0, line.Length - 2);
                }
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            if (clients == null) clients = new ArrayList();
            if (landscape == null) landscape = new Landscape(257);
        }
    }
}