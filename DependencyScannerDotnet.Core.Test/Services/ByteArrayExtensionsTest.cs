using DependencyScannerDotnet.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DependencyScannerDotnet.Core.Test.Services
{
    public class ByteArrayExtensionsTest
    {
        public ByteArrayExtensionsTest() { }

        [Fact]
        public void GetRidOfBom_Ok()
        {
            byte[] bytes = new byte[] { 239, 187, 191, 32, 64, 128 };
            byte[] clearedBytes = bytes.GetRidOfBom();

            Assert.Collection(
                clearedBytes,
                x => Assert.Equal(32, x),
                x => Assert.Equal(64, x),
                x => Assert.Equal(128, x)
            );

            bytes = new byte[] { 239, 187, 32, 64, 128 };
            clearedBytes = bytes.GetRidOfBom();

            Assert.Collection(
                clearedBytes,
                x => Assert.Equal(239, x),
                x => Assert.Equal(187, x),
                x => Assert.Equal(32, x),
                x => Assert.Equal(64, x),
                x => Assert.Equal(128, x)
            );
        }
    }
}
