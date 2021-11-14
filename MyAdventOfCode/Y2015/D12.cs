using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyAdventOfCode.Common;
using MyAdventOfCode.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    public class D12
    {
        private readonly ITestOutputHelper _output;
        public D12(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("[1,2,3]", 6)]
        [InlineData("{\"a\":2,\"b\":4}", 6)]
        [InlineData("[[[3]]]", 3)]
        [InlineData("{\"a\":{\"b\":4},\"c\":-1}", 3)]
        [InlineData("{\"a\":[-1,1]}", 0)]
        [InlineData("[-1,{\"a\":1}]", 0)]
        [InlineData("[]", 0)]
        [InlineData("{}", 0)]
        public void Part_1_Examples(string input, int expectedSum)
            => Run_Part_X_Example(input, expectedSum, Part.One);

        [Fact]
        public async Task Part_1()
            => await Run_Part_X(Part.One);

        [Theory]
        [InlineData("[1,2,3]", 6)]
        [InlineData("[1,{\"c\":\"red\",\"b\":2},3]", 4)]
        [InlineData("{\"d\":\"red\",\"e\":[1,2,3,4],\"f\":5}", 0)]
        [InlineData("[1,\"red\",5]", 6)]
        public void Part_2_Examples(string input, int expectedSum)
            => Run_Part_X_Example(input, expectedSum, Part.Two);

        [Fact]
        public async Task Part_2()
            => await Run_Part_X(Part.Two);

        private void Run_Part_X_Example(string input, int expectedSum, Part part)
        {
            var token = JToken.Parse(input);
            var values = GetAllIntegers(token, part);
            var result = values.Sum();
            Assert.Equal(expectedSum, result);
        }

        private async Task Run_Part_X(Part part)
        {
            var data = await File.ReadAllTextAsync("Data/Y2015/D12.txt");
            var token = JToken.Parse(data);
            var values = GetAllIntegers(token, part);
            var result = values.Sum();
            _output.WriteLine(result);
        }

        private IEnumerable<int> GetAllIntegers(JToken token, Part part)
        {
            // For the second part, we want o ignore the int values from any object 
            // that has a property with a string value "red".
            // so if the token we're processing is an object which has a property whos 
            // value is "red", we want to skip the whole object
            if (part == Part.Two)
            {
                if (token.Type == JTokenType.Object)
                {
                    foreach (var child in token.Children())
                    {
                        if (child.Type == JTokenType.Property)
                        {
                            foreach (var grandChild in child.Children())
                            {
                                if (grandChild.Type == JTokenType.String && grandChild.Value<string>() == "red")
                                {
                                    yield break;
                                }
                            }
                        }
                    }
                }
            }

            // If it's an int, yield the value
            if (token.Type == JTokenType.Integer)
            {
                yield return token.Value<int>();
            }

            // Loop through the children 
            foreach (var child in token.Children())
            {
                // call back through to this method with the children to fetch 
                // the numbers and yield them
                foreach (var childInt in GetAllIntegers(child, part))
                {
                    yield return childInt;
                }
            }
        }
    }
}