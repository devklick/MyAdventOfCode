using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    public class D11
    {
        private readonly ITestOutputHelper _output;
        public D11(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("abcdefgh", "abcdffaa")]
        [InlineData("ghijklmn", "ghjaabcc")]
        public void Part_1_Examples(string input, string expected)
        {
            var generator = PasswordGenerator.Part1;
            var result = generator.RefreshPassword(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Part_1()
        {
            var input = "vzbxkghb";
            var generator = PasswordGenerator.Part1;
            var result = generator.RefreshPassword(input);
            _output.WriteLine(result);
        }

        private class PasswordGenerator
        {
            private readonly IEnumerable<ICheck> _checks;
            private PasswordGenerator(params ICheck[] checks)
            {
                _checks = checks;
            }

            public static PasswordGenerator Part1 =>
                new PasswordGenerator(
                    new LengthCheck(),
                    new CaseCheck(),
                    new SequentialCharsCheck(),
                    new DisallowedCharsCheck(),
                    new GroupedCharactersCheck());

            public string RefreshPassword(string password)
            {
                string newPassword = password;
                do
                {
                    newPassword = IncrementLastCharacter(newPassword);

                } while (!CheckPassword(newPassword));

                return newPassword;
            }
            public bool CheckPassword(string password)
                => _checks.All(check => check.Perform(password));

            private string IncrementLastCharacter(string current)
            {
                var newValueBuilder = new StringBuilder(current);
                var charToIncrement = newValueBuilder.Length - 1;

                void incrementChar() => newValueBuilder[charToIncrement] = (char)(newValueBuilder[charToIncrement] + 1);
                if (newValueBuilder[charToIncrement] < 'z')
                {
                    incrementChar();
                    return newValueBuilder.ToString();
                }

                while (newValueBuilder[charToIncrement] == 'z')
                {
                    newValueBuilder[charToIncrement] = 'a';
                    charToIncrement--;
                }

                incrementChar();

                return newValueBuilder.ToString();
            }
        }

        private interface ICheck
        {
            bool Perform(string input);
        }
        private class LengthCheck : ICheck
        {
            private int _minLengthRequired = 8;
            private int _maxLengthRequired = 8;
            public bool Perform(string input)
                => input.Length >= _minLengthRequired && input.Length <= _maxLengthRequired;
        }

        private class CaseCheck : ICheck
        {
            private enum RequirementLevel { Required, Optional, NotAllowed };
            private RequirementLevel _lowercase = RequirementLevel.Required;
            private RequirementLevel _uppercase = RequirementLevel.NotAllowed;
            public bool Perform(string input)
            {
                bool hasUpper = false, hasLower = false;
                foreach (var c in input)
                {
                    if (char.IsUpper(c))
                    {
                        hasUpper = true;
                    }
                    else if (char.IsLower(c))
                    {
                        hasLower = true;
                    }
                    if (hasLower && hasUpper)
                    {
                        break;
                    }
                }
                if (_uppercase == RequirementLevel.NotAllowed && hasUpper)
                {
                    return false;
                }
                if (_uppercase == RequirementLevel.Required && !hasUpper)
                {
                    return false;
                }
                if (_lowercase == RequirementLevel.NotAllowed && hasLower)
                {
                    return false;
                }
                if (_lowercase == RequirementLevel.Required && !hasLower)
                {
                    return false;
                }
                return true;
            }
        }

        private class SequentialCharsCheck : ICheck
        {
            private int _numberOfSequencesRequired = 1;
            private int _sequenceLengthRequired = 3;
            public bool Perform(string input)
            {
                var sequencesFound = 0;
                for (var i = 0; i < input.Length; i++)
                {
                    if (i + _sequenceLengthRequired > input.Length)
                    {
                        return false;
                    }
                    var one = input[i];
                    var two = input[i + 1];
                    var three = input[i + 2];
                    if (one + 1 == two && two + 1 == three)
                    {
                        sequencesFound++;
                        if (sequencesFound >= _numberOfSequencesRequired)
                        {
                            return true;
                        }
                    }
                }
                return sequencesFound >= _sequenceLengthRequired;
            }
        }

        private class DisallowedCharsCheck : ICheck
        {
            private readonly char[] _disallowedChars = new[] { 'i', 'o', 'l' };

            public bool Perform(string input)
            {
                if (input.Intersect(_disallowedChars).Any())
                {
                    return false;
                }
                return true;
            }
        }

        private class GroupedCharactersCheck : ICheck
        {
            private int _groupLengthRequired = 2;
            private int _numberOfGroupsRequired = 2;
            private bool _groupsMustBeUnique = true;

            public bool Perform(string input)
            {
                var groups = new List<CharacterGrouping>();
                for (var startPosition = 0; startPosition < input.Length; startPosition++)
                {
                    if (startPosition + _groupLengthRequired > input.Length)
                    {
                        break;
                    }
                    var subString = input.Substring(startPosition, _groupLengthRequired);
                    if (subString.Distinct().Count() == 1)
                    {
                        var character = subString.First();
                        if (_groupsMustBeUnique && groups.Any(group => group.Character == character))
                        {
                            continue;
                        }
                        var group = new CharacterGrouping
                        {
                            Character = subString.First(),
                            Indexes = Enumerable.Range(startPosition, _groupLengthRequired).ToList()
                        };
                        groups.Add(group);

                        if (groups.Count() >= _numberOfGroupsRequired)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            private class CharacterGrouping
            {
                public char Character { get; set; }
                public List<int> Indexes { get; set; }
            }
        }
    }
}