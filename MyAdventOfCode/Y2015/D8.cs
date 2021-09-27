using MyAdventOfCode.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    public class D8
    {
        private readonly ITestOutputHelper _output;
        public D8(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Part_1()
        {
            var data = await GetTestData();

            var codeLength = 0;
            var memoryLength = 0;

            foreach(var row in data)
            {
                codeLength += row.Length;
                memoryLength += Regex.Unescape(row.Substring(1, row.Length - 2)).Length;
            }

            var diff = codeLength - memoryLength;

            _output.WriteLine(diff);
        }

        [Fact]
        public async Task Part_2()
        {
            var data = await GetTestData();

            var codeLength = 0;
            var memoryLength = 0;

            foreach (var row in data)
            {
                codeLength += row.Length;
                memoryLength += row.Replace("\\", "\\\\").Replace("\"", "\\\"").Length + 2;
            }

            var diff = memoryLength - codeLength;

            _output.WriteLine(diff);
        }

        private async Task<IEnumerable<string>> GetTestData()
        {
            var data = await File.ReadAllTextAsync("Data/Y2015/D8.txt", Encoding.ASCII);
            return data.Split(Environment.NewLine);
        }
    }
}
