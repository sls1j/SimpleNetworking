using System.Net.Sockets;
using System.Net;
using System.Text;

List<Receiver> receivers = new List<Receiver>();
TcpListener listener = new TcpListener(IPAddress.Any, 45001);
listener.Start();
while (true)
{
  Console.WriteLine("Waiting for client to connect...");
  TcpClient client = listener.AcceptTcpClient();
  Console.WriteLine($"New client connected via {client.Client.RemoteEndPoint.ToString()}");
  receivers.Add(new Receiver(client));
}

// You can define other methods, fields, classes and namespaces here

class Receiver
{
  private TcpClient _client;
  private NetworkStream _stream;
  public Receiver(TcpClient client)
  {
    _client = client;
    _stream = _client.GetStream();

    // starts the loop to read messages from the client
    ThreadPool.QueueUserWorkItem(MessageLoop);
  }

  private void MessageLoop(object o)
  {
    Console.WriteLine($"Is Connected? {_client.Connected.ToString()}");
    using NetworkStream stream = _client.GetStream();
    while (_client.Connected)
    {
      string message = ReadMessage();
      if (!string.IsNullOrEmpty(message))
      {
        // write out response
        string outMessage = $"ACK {message}";
        WriteMessage(outMessage);
      }
      else
        break;
    }

    Console.WriteLine("Client disconnected");
  }

  string ReadMessage()
  {
    // read into a buffer
    int count = 0;
    StringBuilder message = new StringBuilder();
    try
    {
      while (true)
      {
        int b = _stream.ReadByte();
        if (b <= 0)
          break;
        else
        {
          message.Append((char)b);
          count++;
        }

        if (count > 4096)
          throw new InvalidOperationException("Message too big! Aborting connection");
      }
    }
    catch (Exception)
    {
      // if something goes wrong we'll just swallow the error
      Console.WriteLine("Failed to read.");
    }
    
    string m = message.ToString();
    if (!string.IsNullOrEmpty(m))
    {
      Console.WriteLine($"Received: {m}");
    }

    return m;
  }

  void WriteMessage(string message)
  {
    try
    {
      byte[] buffer = Encoding.UTF8.GetBytes(message);
      _stream.Write(buffer, 0, buffer.Length);
      _stream.WriteByte(0); // this tells the stream on the other side the message is done.

      Console.WriteLine($"Send: {message}");
    }
    catch (Exception)
    {
      Console.WriteLine("Failed to send message");
    }
  }
}