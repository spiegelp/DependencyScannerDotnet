using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public static class ByteArrayExtensions
    {
        public static byte[] GetRidOfBom(this byte[] bytes)
        {
            // get rid of the BOM (XML API does not like it)
            if (bytes.Length > 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
            {
                bytes = bytes[3..];
            }

            return bytes;
        }
    }
}
