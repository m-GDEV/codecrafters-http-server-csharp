using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

// -----------------------------------------
// Random helper functions
// -----------------------------------------
byte[] generateResponse(string status, string contentType, string responseBody, string? encoding = null) {
    // Status Line
    string response = $"HTTP/1.1 {status}\r\n";

    // Headers
    response += $"Content-Type: {contentType}\r\n";
    response += $"Content-Length: {responseBody.Length}\r\n";
    response += "\r\n";

    // Content Encoding
    if (encoding != null) {
        response += $"Content-Encoding: {encoding}";
    }

    // Response Body
    response += responseBody;

    return Encoding.UTF8.GetBytes(response);
}

string readFile(string filepath) {
    // Handling exceptions "higher" up
    string fileContents = "";

    // Read file using stream
    StreamReader fp = new StreamReader(filepath);
    var line = fp.ReadLine();

    while (line != null) {
        fileContents += line;
        line = fp.ReadLine();
    }
    fp.Close();

    return fileContents;
}

void writeFile(string filepath, string fileContents) {
    StreamWriter fp = new StreamWriter(filepath);

    // We need to remove the trailing null terminator. I didn't think this would be an issue in C# lol
    fp.Write(fileContents.Replace("\0", string.Empty));
    fp.Close();
}

// -----------------------------------------
// Methods to handle different HTPP methods
// -----------------------------------------
byte[] handleGET(string[] parsedLines) {
    // Setup stuff
    string path = parsedLines[0].Split(" ")[1]; // /echo/apple
    string userAgent = parsedLines[2].Split(" ")[1];
    string encoding = parsedLines[3].Split(" ")[1];

    // Branching logic
    if (path.Equals("/")) {
        return generateResponse("200 OK", "text/plain", "Nothing");
    }

    // Return if file specified after '/files/' exists, return contents in resonse body
    else if (path.StartsWith("/files/")) {
        // Instructions mention the program WILL be run like this ./program --directory dir
        string directoryName = args[1];
        string filename = path.Split("/")[2];

        try {
            string fileContents = readFile(directoryName + filename);
            return generateResponse("200 OK", "application/octet-stream", fileContents);
        }
        catch (Exception){
            return generateResponse("404 Not Found", "text/plain", "File Not Found");
        }
    }

    // Return User-Agent in resonse body
    else if (path.Equals("/user-agent")) {
        return generateResponse("200 OK", "text/plain", userAgent);
    }

    // Return text after '/echo/' in resonse body
    else if (path.StartsWith("/echo")) {
        string word = path.Split("/")[2];

        if (encoding == "gzip") {
            return generateResponse("200 OK", "text/plain", word, "gzip");
        }
        return generateResponse("200 OK", "text/plain", word);
    }

    // You're a loser
    else {
        return generateResponse("404 Not Found", "text/plain", "Nothing Dipshit");
    }
}

byte[] handlePOST(string[] parsedLines) {
    // Setup stuff
    string path = parsedLines[0].Split(" ")[1]; // /echo/apple
    string body = parsedLines[4];

    // Return if file specified after '/files/' exists, return contents in resonse body
    if (path.StartsWith("/files/")) {
        // Instructions mention the program WILL be run like this ./program --directory dir
        string directoryName = args[1];
        string filename = path.Split("/")[2];

        try {
            writeFile(directoryName + filename, body);
            return generateResponse("201 Created", "text/plain", "Nothing");
        }
        catch (Exception){
            return generateResponse("404 Not Found", "text/plain", "Can't Write File");
        }
    }

    // You're a loser
    else {
        return generateResponse("404 Not Found", "text/plain", "Nothing Dipshit");
    }
}

// -----------------------------------------
// Main Code
// -----------------------------------------

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Create TcpListener
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true) {
    // Create new socket
    Socket client =  server.AcceptSocket();
    Console.WriteLine("Connection Established");

    // Create response buffer and get resonse (lol idk how to dynamically handle this)
    byte[] requestText = new byte[1000];
    client.Receive(requestText);

    // Parse request path
    string parsed = System.Text.Encoding.UTF8.GetString(requestText);
    // Console.WriteLine(parsed);
    string[] parsedLines = parsed.Split("\r\n");
    string method = parsedLines[0].Split(" ")[0]; // GET, POST

    // Logic
    switch (method) {
        case "GET":
            client.Send(handleGET(parsedLines));
            break;
        case "POST":
            client.Send(handlePOST(parsedLines));
            break;
    }

    client.Close();
}
// server.Stop();
