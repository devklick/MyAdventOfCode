using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MyAdventOfCode.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    /// <summary>
    /// <see href="https://adventofcode.com/2015/day/4"/>
    /// <remarks>
    /// This one has a particularly vague context, so the solution is equally vague.
    /// </remarks>
    /// </summary>
    public class D4
    {
        private readonly ITestOutputHelper _output;
        public D4(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("abcdef", 609043)]
        [InlineData("pqrstuv", 1048970)]
        public void Part_1_Example(string secretKey, long expected)
        {
            var hacker = new AdventCoinMiner(secretKey);
            var result = hacker.Mine(maxTries: expected);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Part_1()
        {
            var hacker = new AdventCoinMiner("iwrupvqb");
            var result = hacker.Mine();
            _output.WriteLine(result);
        }
        
        [Fact]
        public void Part_2()
        {
            var hacker = new AdventCoinMiner("iwrupvqb");
            var result = hacker.Mine(successfulHexStartString: "000000");
            _output.WriteLine(result);
        }

        private class AdventCoinMiner 
        {
            private readonly string _secretKey;
            public AdventCoinMiner(string secretKey)
            {
                _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            }

            public long Mine(long? maxTries = null, string successfulHexStartString = "00000")
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    for (int i = 0; true; i++)
                    {
                        var inputBytes = Encoding.ASCII.GetBytes(_secretKey + i);
                        var hashBytes = md5.ComputeHash(inputBytes);

                        var sb = new StringBuilder();
                        foreach (var hashByte in hashBytes)
                        {
                            sb.Append(hashByte.ToString("X2"));
                        }

                        var hex = sb.ToString();
                        if (hex.Length >= 5 && hex.Substring(0, successfulHexStartString.Length) == successfulHexStartString)
                        {
                            return i;
                        }
                        
                        // Added while debugging the example tests, to prevent getting stuck in an infinite loop
                        if (i >= maxTries)
                        {
                            throw new Exception("Hit max tries, failed to hack secret key");
                        }
                    }   
                }
            }
        }
    }
}