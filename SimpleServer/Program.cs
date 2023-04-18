using System.Net.Sockets;
using System.Net;
using System.Text;

List<Receiver> receivers = new List<Receiver>();

// setup a server that listens to port 45001 on all available networks (localhost, lan, wifi, or whatever else is available)
TcpListener listener = new TcpListener(IPAddress.Any, 45001);
listener.Start();
while (true)
{
  Console.WriteLine("Waiting for client to connect...");
  TcpClient client = listener.AcceptTcpClient();

  // add to list of receivers
  Console.WriteLine($"New client connected via {client.Client.RemoteEndPoint.ToString()}");
  receivers.Add(new Receiver(client));

  // remove all the dead client
  receivers.RemoveAll(c => c.isDead);
}

// Defines the code to handle the messages coming in from a single connection
class Receiver
{
  private TcpClient _client;
  private NetworkStream _stream;
  public Receiver(TcpClient client)
  {
    _client = client;
    _stream = _client.GetStream();

    // starts the loop in a thread to read messages from the client
    ThreadPool.QueueUserWorkItem(MessageLoop);
  }

  public bool isDead { get; private set; } = false;

  /// <summary>
  /// The loop that listens for messages one at a time
  /// </summary>
  /// <param name="o">No used</param>
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
    isDead = true;
  }

  /// <summary>
  /// Code to read a 0 terminated string from the socket
  /// </summary>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException">When bad things happen this is thrown</exception>
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

  /// <summary>
  /// Writes a message to the connection and ends it with a 0
  /// </summary>
  /// <param name="message">The message</param>
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