using System.Net;
using System.Net.Sockets;
using System.Text;

byte[] generateResponse(string status, string contentType, string responseBody) {
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

    // Create response buffer and get resonse
    byte[] requestText = new byte[100];
    client.Receive(requestText);

    // Parse request path
    string parsed = System.Text.Encoding.UTF8.GetString(requestText);

    string[] parsedLines = parsed.Split("\r\n");
    // Console.WriteLine(parsed);

    // Capturing specific parts of the request
    string method = parsedLines[0].Split(" ")[0]; // GET, POST
    string path = parsedLines[0].Split(" ")[1]; // /echo/apple

    string userAgent = parsedLines[2].Split(" ")[1];

    // Console.WriteLine($"agent is: {userAgent}");

    // Logic
    if (path.Equals("/")) {
        client.Send(generateResponse("200 OK", "text/plain", "Nothing"));
    }
    else if (path.Equals("/user-agent")) {
        client.Send(generateResponse("200 OK", "text/plain", userAgent));
    }
    else if (path.StartsWith("/echo")) {
        string[] words = path.Split("/");
        client.Send(generateResponse("200 OK", "text/plain", words[2]));
    }
    else {
        client.Send(generateResponse("404 Not Found", "text/plain", "Nothing Dipshit"));
    }

    client.Close();

}


server.Stop();
