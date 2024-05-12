using System.Net;
using System.Net.Sockets;
using System.Text;

byte[] generatedReturnByteArray(string status, string contentType, string responseBody) {
    // Status Line
    string response = $"HTTP/1.1 {status}\r\n";

    // Headers
    response += $"Content-Type: {contentType}\r\n";
    response += $"Content-Length: {responseBody.Length}\r\n";
    response += "\r\n";

    // Response Body
    response += responseBody;

    return Encoding.UTF8.GetBytes(response);
}


// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Create TcpListener
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true) {
    // Create new socket
    Socket client =  server.AcceptSocket();
    Console.WriteLine("Connection Established");

    byte[] request_text = new byte[100];
    client.Receive(request_text);

    string parsed = System.Text.Encoding.UTF8.GetString(request_text);
    string[] parsed_lines = parsed.Split("\r\n");
    string[] path_words = parsed_lines[0].Split(" ");
    string path = path_words[1];

    Console.WriteLine($"Path is: {path}");

    if (path.StartsWith("/echo")) {
        string[] words = path.Split("/");
        client.Send(generatedReturnByteArray("200 OK", "text/plain", words[2]));
    }
    else {
        client.Send(generatedReturnByteArray("404 Not Found", "text/plain", "Nothing Dipshit"));
    }

    client.Close();

}


server.Stop();
