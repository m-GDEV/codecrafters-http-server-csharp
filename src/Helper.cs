using System.Text;
using System.IO.Compression;
using Types;

namespace Helper;

public class Functions {

    /*
       Generate the byte array that is the HTTP response of the server
       */
    public static byte[] generateResponse(string status, string contentType, string responseBody, string? encoding = null) {
        // Status Line
        string response = $"HTTP/1.1 {status}\r\n";

        // Headers
        response += $"Content-Type: {contentType}\r\n";
        response += $"Content-Length: {responseBody.Length}\r\n";

        // Content Encoding and compression
        if (encoding != null) {
            response += $"Content-Encoding: {encoding}\r\n";
            response += "\r\n";

            response += Compress(responseBody);

            Console.WriteLine($"compressed: {Compress(responseBody)}");


        }
        // No compression
        else {
            response += "\r\n";
            response += responseBody;
        }

        return Encoding.UTF8.GetBytes(response);
    }


    /*
       Given a string, compress it using gzip and return that in string form
       */
    public static string Compress(string info) {
        // Compressing the body
        byte[] data = Encoding.UTF8.GetBytes(info);

        MemoryStream compressedBody = new MemoryStream();
        GZipStream compressor = new GZipStream(compressedBody, CompressionMode.Compress);
        compressor.Write(data, 0, data.Length);
        compressor.Close();

        return Encoding.UTF8.GetString(compressedBody.ToArray());
    }


    /*
       Read a file from the specified location and return it in a string
       */
    public static string readFile(string filepath) {
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


    /*
       Write a file to the specified location
       */
    public static void writeFile(string filepath, string fileContents) {
        StreamWriter fp = new StreamWriter(filepath);

        // We need to remove the trailing null terminator. I didn't think this would be an issue in C# lol
        fp.Write(fileContents.Replace("\0", string.Empty));
        fp.Close();
    }

    /*
       Handle a GET request, returns a byte array of the response
       */
    public static byte[] handleGET(Headers requestHeaders, string directoryName) {
        // Branching logic
        if (requestHeaders.Path.Equals("/")) {
            return generateResponse("200 OK", "text/plain", "Nothing");
        }

        // Return if file specified after '/files/' exists, return contents in resonse body
        else if (requestHeaders.Path.StartsWith("/files/")) {
            string filename = requestHeaders.Path.Split("/")[2];

            try {
                string fileContents = readFile(directoryName + filename);
                return generateResponse("200 OK", "application/octet-stream", fileContents);
            }
            catch (Exception){
                return generateResponse("404 Not Found", "text/plain", "File Not Found");
            }
        }

        // Return User-Agent in resonse body
        else if (requestHeaders.Path.Equals("/user-agent")) {
            return generateResponse("200 OK", "text/plain", requestHeaders.UserAgent);
        }

        // Return text after '/echo/' in resonse body
        else if (requestHeaders.Path.StartsWith("/echo")) {
            string word = requestHeaders.Path.Split("/")[2];

            if (requestHeaders.Encoding != null && requestHeaders.Encoding != "" && requestHeaders.Encoding.Equals("gzip")) {
                return generateResponse("200 OK", "text/plain", "", "gzip");
            }
            return generateResponse("200 OK", "text/plain", word);
        }

        // You're a loser
        else {
            return generateResponse("404 Not Found", "text/plain", "Nothing Dipshit");
        }
    }


    /*
       Handle a POST request, returns a byte array of the response
       */
    public static byte[] handlePOST(Headers requestHeaders, string directoryName) {
        // Return if file specified after '/files/' exists, return contents in resonse body
        if (requestHeaders.Path.StartsWith("/files/")) {
            // Instructions mention the program WILL be run like this ./program --directory dir
            string filename = requestHeaders.Path.Split("/")[2];

            try {
                writeFile(directoryName + filename, requestHeaders.Body);
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

    /*
       Process headers from the request and return a struct representing them
       */
    public static Headers processHeaders(string parsed) {
        string[] parsedLines = parsed.Split("\r\n");
        Headers requestHeaders = new Headers();

        //
        foreach (var header in parsedLines) {
            if (header.StartsWith("GET") || header.StartsWith("POST")) {
                requestHeaders.Method = getMethod(header);
                requestHeaders.Path = getPath(header);
            }
            // Headers are case insensitve
            else if (header.ToLower().StartsWith("user-agent:")){
                requestHeaders.UserAgent = getUserAgent(header);
            }
            else if (header.ToLower().StartsWith("accept-encoding:")){
                requestHeaders.Encoding = getEncoding(header);
            }
            else if (header.ToLower().StartsWith("content-length:")) {
                // nothing
            }
            else if (header.ToLower().StartsWith("host:")) {
                // nothing
            }

            // Catch all at the end since it should be the body by now. If its not i'm missing a header to check for above
            else {
                requestHeaders.Body += header;
            }
        }

        return requestHeaders;
    }

    /*
       Helper functions to process each individual header
       */
    public static string getMethod(string headerString) {
        return headerString.Split(" ") [0];
    }
    public static string getPath(string headerString) {
        return headerString.Split(" ") [1];
    }
    public static string getUserAgent(string headerString) {
        return headerString.Split(" ") [1];
    }
    public static string getEncoding(string headerString) {
        foreach (var encoding in headerString.Split(" ")) {
            if (encoding.Equals("gzip,") || encoding.Equals("gzip")) {
                return "gzip";
            }
        }

        return "";
    }

}


