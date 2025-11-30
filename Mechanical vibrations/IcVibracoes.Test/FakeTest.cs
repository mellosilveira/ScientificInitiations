using System;
using IcVibracoes.Core.ExtensionMethods;
using Xunit;

namespace IcVibracoes.Test
{
    public class FakeTest
    {
        [Fact]
        public void Test()
        {
            uint[] vector = new uint[10];

            uint initialValue = 1;
            Array.Fill(vector, initialValue++);


            // Assert

        }
    }
}
