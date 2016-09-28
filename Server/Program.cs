using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using System.Globalization;
using System.Diagnostics;

namespace JustShootServer
{
  class Server
  {
    private static int listenport = 1978;
    private static TcpListener listener;
    public static ArrayList clients;
    private static Thread processor;
    private static Socket clientsocket;
    private static Thread clientservice;
    private static NetworkStream clientstream;
    private static TcpClient tcpclient;
    private static bool run = true;
    private static List<byte> buf = new List<byte>();
    public static Landscape landscape;
    private static Timer engineticker;
    private static Timer opponentticker;
    private static Timer positionticker;
    public static List<Bullet> bullets = new List<Bullet>();
    private static bool positionticking = false;
    private static bool bulletarraylocked = false;
    //    private static byte[] testdata = { 0x81, 0x9c, 0xf8, 0xe2, 0x4c, 0xaa, 0xaa, 0x8d, 0x2f, 0xc1, 0xd8, 0x8b, 0x38, 0x8a, 0x8f, 0x8b, 0x38, 0xc2, 0xd8, 0xaa, 0x18, 0xe7, 0xb4, 0xd7, 0x6c, 0xfd, 0x9d, 0x80, 0x1f, 0xc5, 0x9b, 0x89, 0x29, 0xde };
    List<Vehicle> vehicles = new List<Vehicle>();
    private static readonly object _syncObject = new object();

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

    public static String ComputeWebSocketHandshakeSecurityHash09(String secWebSocketKey)
    {
      const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
      String secWebSocketAccept = String.Empty;
      String ret = secWebSocketKey + MagicKEY; // 1. Combine the request Sec-WebSocket-Key with magic key.
      SHA1 sha = new SHA1CryptoServiceProvider(); // 2. Compute the SHA1 hash
      byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));
      secWebSocketAccept = Convert.ToBase64String(sha1Hash); // 3. Base64 encode the hash
      return secWebSocketAccept;
    }

    private static void StartListening()
    {
      listener = new TcpListener(IPAddress.Any, listenport);
      listener.Start();
      while (run)
      {
        try
        {
          TcpClient client = listener.AcceptTcpClient();
          //Socket s = listener.AcceptSocket();

          var headers = new Dictionary<string, string>();
          string line = string.Empty;
          NetworkStream stream = client.GetStream();
          while ((line = ReadLine(stream)) != string.Empty)
          {
            var tokens = line.Split(new char[] { ':' }, 2);
            if (line != null && line.Trim().Length > 0 && tokens.Length > 1)
            {
              headers[tokens[0]] = tokens[1].Trim();
              Log(tokens[0] + ": " + tokens[1], ConsoleColor.DarkYellow);
            }
          }

          string responsekey = ComputeWebSocketHandshakeSecurityHash09(headers["Sec-WebSocket-Key"]);

          var result = new List<byte>();

          var response =
              "HTTP/1.1 101 WebSocket Protocol Handshake\r\n" +
              "Upgrade: WebSocket\r\n" +
              "Connection: Upgrade\r\n" +
              "Sec-WebSocket-Accept: " + responsekey + "\r\n" +
              "Server:  Tank Dual\r\n" +
              "Date:  Mon, 02 Apr 2012  09:54:59 GMT\r\n" +
              "Access-Control-Allow-Origin: " + headers["Origin"] + "\r\n" +
              "Access-Control-Allow-Credentials: true\r\n" +
              "Access-Control-Allow-Headers: content-type\r\n" +

              /*"Sec-WebSocket-Origin: " + headers["Origin"] + Environment.NewLine +
              "Sec-WebSocket-Location: ws://localhost:1978/chat" + Environment.NewLine +*/
              "\r\n";
          /*
           * 
*/
          var bufferedResponse = Encoding.UTF8.GetBytes(response);
          stream.Write(bufferedResponse, 0, bufferedResponse.Length);
          /*
          using (var md5 = MD5.Create())
          {
            var handshake = md5.ComputeHash(result.ToArray());
            stream.Write(handshake, 0, handshake.Length);
          }
          */
          clientsocket = client.Client;
          tcpclient = client;
          clientstream = stream;
          clientservice = new Thread(new ThreadStart(ServiceClient));
          clientservice.Start();
          Log("Client connected, waiting fore name", ConsoleColor.DarkYellow);
          Log("* sending landscape", ConsoleColor.DarkYellow);
          sendText(stream, "{ \"type\": \"chat\", \"id\": -1, \"text\": \"Receiving landscape\"}");
          sendText(stream, "{ \"type\": \"landscape\", \"data\": " + landscape.toJSON() + "}");
          Log("* sending client list", ConsoleColor.DarkYellow);
          sendText(stream, "{ \"type\": \"chat\", \"id\": -1, \"text\": \"Receiving client list\"}");
          System.Threading.Thread.Sleep(100);
          sendText(stream, GetClientListJSON());
        }
        catch (Exception e)
        {
          Log(e.ToString(), ConsoleColor.Red);
        }
      }
      listener.Stop();
    }

    private static bool sendText(NetworkStream stream, string text)
    {
      try
      {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
        List<byte> buffer = new List<byte>();
        byte b1 = (byte)((byte)0x80 | ((byte)Frame.Opcode.Text & 0x0f));
        buffer.Add(b1);
        if (data.Length <= 125)
          buffer.Add((byte)(data.Length));
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
        byte[] mask = new byte[4];
        Random random = new Random();
        /*
              for (int i = 0; i < 4; i++)
              {
                mask[i] = (byte)random.Next(0, 255);
                buffer.Add(mask[i]);
              }*/
        for (int i = 0; i < data.Length; i++)
          buffer.Add((byte)(data[i]/* ^ mask[i % 4]*/));
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
    private static void ServiceClient()
    {
      //Socket client = clientsocket;
      bool keepalive = true;
      NetworkStream stream = clientstream;
      int id = getVehicleId();
      Client client = new Client(id, null, System.Threading.Thread.CurrentThread, stream, Client.VehicleType.Tank);
      clients.Add(client);
      sendText(stream, "{ \"type\": \"id\", \"id\": " + client.Id + ", \"name\": null}");
      while (keepalive)
      {
        try
        {
          Frame frame = new Frame(stream);
          switch (frame.getOpcode())
          {
            case Frame.Opcode.Close:
            case Frame.Opcode.Disconnect:
              keepalive = false;
              break;
            case Frame.Opcode.Text:
              string text = frame.getText();
              if (!string.IsNullOrEmpty(text))
                processMessage(client, stream, text);
              break;
          }
        }
        catch (Exception ex)
        {
          keepalive = false;
          Log(ex.ToString(), ConsoleColor.Red);
        }
      }
      Client remove = null;
      foreach (Client c in clients)
      {
        if (c.CLThread == System.Threading.Thread.CurrentThread)
        {
          remove = c;
          break;
        }
      }
      if (remove != null)
      {
        remove.Stream.Close();
        clients.Remove(remove);
        sendToAllClients(client, "{ \"type\": \"quit\", \"id\": \"" + remove.Id + "\"}");
        Log("* " + remove.Name + " quit", ConsoleColor.Blue);
      }
    }

    public static int getVehicleId()
    {
      int available = 0;
      for (int n = 0; n < clients.Count; n++)
      {
        Client cl = (Client)clients[n];
        available = Math.Max(cl.Id, available);
      }
      available++;
      return available;
    }

    public static int getBulletId()
    {
      int available = 0;
      for (int n = 0; n < bullets.Count; n++)
      {
        Bullet b = (Bullet)bullets[n];
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
        if ((c == null || cl.Id != c.Id) && cl.Stream != null) // Do not send message to self or one that has no name
        {
          try
          {
            sendText(cl.Stream, json);
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

    private static void processMessage(Client client, NetworkStream stream, string message)
    {
      if (message.ToLower().StartsWith("/"))
      {
        if (message.ToLower().StartsWith("/nick"))
        {
          string name = message.Substring(6);
          bool nametaken = false;
          foreach (Client c in clients)
          {
            if (client.Name != null && c.Name.ToLower().CompareTo(name.ToLower()) == 0)
            {
              nametaken = true;
              break;
            }
          }
          if (nametaken)
          {
            sendText(stream, "{ \"type\": \"error\", \"text\": \"Nickname already in use\"}");
            Log("* Name " + name + " taken", ConsoleColor.DarkYellow);
          }
          else
          {
            //            string json = "{\"type\":\"join\",\"name\":\"" + name + "\"}";
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
              sendText(stream, "{ \"type\": \"chat\", \"id\": -1, \"text\": \"Name accepted\"}");
              sendText(stream, "{ \"type\": \"id\", \"id\": " + client.Id + ", \"name\": \"" + name + "\"}");
              sendToAllClients(client, "{ \"type\": \"join\", \"id\": " + client.Id + ", \"name\": \"" + name + "\"}");
              client.vehicle.Wake();
            }
          }
        }
        else
        {
          sendText(stream, "{ \"type\": \"error\", \"text\": \"Unknown command: " + message + "\"}");
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
          client.vehicle.functionPressed((Vehicle.VehicleFunction)Int32.Parse(args[0].Substring(1)));
          if (client.vehicle is VehicleTank)
          {
            ((VehicleTank)client.vehicle).barrelmount = new Vector3D(Double.Parse(args[1], CultureInfo.InvariantCulture), Double.Parse(args[2], CultureInfo.InvariantCulture), Double.Parse(args[3], CultureInfo.InvariantCulture));
            ((VehicleTank)client.vehicle).barreldirection = new Vector3D(Double.Parse(args[4], CultureInfo.InvariantCulture), Double.Parse(args[5], CultureInfo.InvariantCulture), Double.Parse(args[6], CultureInfo.InvariantCulture));
          }
        }
      }
      else if (message.ToLower().StartsWith("-")) // Key up
      {
        client.vehicle.functionReleased((Vehicle.VehicleFunction)Int32.Parse(message.Substring(1)));
      }
      else
      {
        string[] c = message.Split(';');
        double x = Double.Parse(c[0], CultureInfo.InvariantCulture);
        double y = Double.Parse(c[1], CultureInfo.InvariantCulture);
        double z = Double.Parse(c[2], CultureInfo.InvariantCulture);
        double r = Double.Parse(c[3], CultureInfo.InvariantCulture);
        double tr = Double.Parse(c[4], CultureInfo.InvariantCulture);
        double br = Double.Parse(c[5], CultureInfo.InvariantCulture);
        //Log("<" + client.Name + "> " + message, ConsoleColor.White);
      }
    }

    private static void SendToClient(Client cl, string message)
    {
      try
      {
        sendText(cl.Stream, message);
      }
      catch
      {
        cl.Stream.Close();
        cl.CLThread.Abort();
        clients.Remove(cl);
        sendToAllClients(null, "{ \"type\": \"quit\", \"id\": \"" + cl.Id + "\"}");
        Log("* " + cl.Name + " quit", ConsoleColor.Blue);
      }
    }

    private static string GetClientListJSON()
    {
      StringBuilder ret = new StringBuilder();
      ret.Append("{\"type\": \"clients\", \"list\": [");
      for (int n = 0; n < clients.Count; n++)
      {
        Client cl = (Client)clients[n];
        if (cl.Name != null)
        {
          if (n > 0)
            ret.Append(",");
          ret.Append("{ \"id\": " + cl.Id + ", \"name\": \"" + cl.Name + "\"}");
        }
      }
      ret.Append("]}");
      return ret.ToString();
    }

    private static Random r = new Random();
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
          if (c.Stream != null)
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
        if (!bulletarraylocked)
        {
          List<int> remove = new List<int>();
          for (int i = 0; i < bullets.Count; i++)
          {
            bullets[i].Tick();
            if (bullets[i].hitGround())
            {
              sendToAllClients(null, "{ \"type\": \"bhit\", \"id\": " + bullets[i].id + ", \"x\": " + bullets[i].position.x.ToString(CultureInfo.InvariantCulture) + ", \"z\": " + bullets[i].position.z.ToString(CultureInfo.InvariantCulture) + "}");
              remove.Add(i);
            }
            foreach (Client c in clients)
            {
              if (bullets[i].hitVehicle(c.vehicle))
              {
                sendToAllClients(null, "{ \"type\": \"vhit\", \"bid\": " + bullets[i].id + ", \"vid\": " + c.vehicle.id + "}");
                c.vehicle.Kill();
                remove.Add(i);
              }
            }
          }
          for (int i = remove.Count - 1; i >= 0; i--)
          {
            if (bullets.Count > remove[i])
              bullets.RemoveAt(remove[i]);
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
        foreach (Client c in clients)
        {
          if (c.Name != null)
            sendToAllClients(c, c.vehicle.toJSON());
        }
        bulletarraylocked = true;
        foreach (Bullet b in bullets)
        {
          sendToAllClients(null, b.toJSON());
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
      foreach (Client c in clients)
      {
        if (c.Stream != null)
          sendText(c.Stream, c.vehicle.toJSON());
      }
    }

    static void Main(string[] args)
    {
      try
      {
        landscape = new Landscape(257);
        clients = new ArrayList();
        processor = new Thread(new ThreadStart(StartListening));
        processor.Start();
        
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
  }
}