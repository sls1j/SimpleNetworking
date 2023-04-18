using System.Net.Sockets;
using System.Text;


Client client = new Client("localhost", 45001);

// send 10 messages
for (int i = 0; i < 10; i++)
{
  // send a message
  client.WriteMessage($"MESSAGE {i}");

  // wait for the response
  string response = client.ReadMessage();
  Console.WriteLine($"Received response: {response}");
}

client.Close();

// Defines the Client logic
class Client
{
  TcpClient _client;
  NetworkStream _stream;

  public Client(string host, int port)
  {
    _client = new TcpClient(host, port);
    _stream = _client.GetStream();
  }

  /// <summary>
  /// Reads data from the socket until a 0 character is received or a -1 (end of stream) is received
  /// </summary>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public string ReadMessage()
  {
    // read into a buffer
    int count = 0;
    StringBuilder message = new StringBuilder();
    while (true)
    {
      int b = _stream.ReadByte();
      if (b == 0)
        break;
      else
      {
        message.Append((char)b);
        count++;
      }

      if (count > 4096)
        throw new InvalidOperationException("Message too big! Aborting connection");
    }

    return message.ToString();
  }

  /// <summary>
  /// Writes data to the socket then a 0 to indicate that it's finished
  /// </summary>
  /// <param name="message"></param>
  public void WriteMessage(string message)
  {
    byte[] buffer = Encoding.UTF8.GetBytes(message);
    _stream.Write(buffer, 0, buffer.Length);
    _stream.WriteByte(0); // this tells the stream on the other side the message is done.
  }

  /// <summary>
  /// Closes the socket
  /// </summary>
  public void Close()
  {
    _client.Close();   
  }
}

// You can define other methods, fields, classes and namespaces here