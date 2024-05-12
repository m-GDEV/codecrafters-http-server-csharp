namespace Types;


// I'm building this so you can add more fields later
public struct Headers {
    public string Method { get; set; }
    public string Path { get; set; }
    public string UserAgent { get; set; }
    public string Encoding { get; set; }
    public string Body { get; set; }

    public override string ToString() {
        return $"Method: {Method}, Path: {Path}, UserAgent: {UserAgent}, Encoding: {Encoding}, Body: {Body}";
    }
}
