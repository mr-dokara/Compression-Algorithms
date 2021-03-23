namespace CA_utils
{
    public interface ICompression
    {
        string Encode(string text);
        string Decode(string text);
    }
}