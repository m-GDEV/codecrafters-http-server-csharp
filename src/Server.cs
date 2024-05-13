using System.Net;
using System.Net.Sockets;

using Helper;
using Types;

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
    Headers requestHeaders = Functions.processHeaders(parsed);

    string directoryName = "";
    if (args.Length >= 2 && args[0] == "--directory") {
        directoryName = args[1];
    }

    Console.WriteLine(parsed);
    Console.WriteLine(requestHeaders);

    // Logic
    switch (requestHeaders.Method) {
        case "GET":
            // Instructions mention the program WILL be run like this ./program --directory dir
            client.Send(Functions.handleGET(requestHeaders, directoryName));
            break;
        case "POST":
            // Instructions mention the program WILL be run like this ./program --directory dir
            client.Send(Functions.handlePOST(requestHeaders, directoryName));
            break;
    }

    client.Close();
}
// server.Stop();




// OK SO YOU NEED TO FIGURE OUT HOW TO PARSE THE HEADERS PROPERLY, NOT BASED ON THEIR POSITION LOL
// i'm thinking, write a function that processes the 'parsed' variable and return a struct including
// the parsed header values
