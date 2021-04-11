using System;
using System.Collections.Generic;
using System.Linq;
using CA_utils;

namespace RLEandBWT
{
    public class RLEandBWT : ICompression
    {
        public double CompressionRatio
        {
            get { return rle.CompressionRatio; }
        }

        public double AverageLength
        {
            get { return rle.AverageLength; }
        }

        private BWT bwt = new BWT();
        private RLE rle = new RLE();

        public string Encode(string text) 
            => rle.Encode(bwt.Encode(text));

        public string Decode(string text) 
            => bwt.Decode(rle.Decode(text));

        public override string ToString()
            => bwt.ToString();
    }
}