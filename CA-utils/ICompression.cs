using System.Threading.Tasks;

namespace CA_utils
{
    public interface ICompression
    {
        double CompressionRatio { get; }

        string Encode(string text);
        string Decode(string text);
        string ToString();
    }
}