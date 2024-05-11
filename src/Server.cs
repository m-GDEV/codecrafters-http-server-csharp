using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
Socket client =  server.AcceptSocket(); // wait for client

Console.WriteLine("Connection Established");

byte[]? request_text = null;
client.Receive(request_text);

Console.WriteLine(request_text);


string return_string = "HTTP/1.1 200 OK\r\n\r\n";
byte[] return_array = System.Text.Encoding.ASCII.GetBytes(return_string);

client.Send(return_array);

server.Stop();
