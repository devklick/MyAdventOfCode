using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MyAdventOfCode.Common;

using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2022.D07;

public class Solution : TestBase
{
    public Solution(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public override async Task Part1_Example()
        => await Invoke(
            Part.One,
            DataType.Example,
            fs => fs.GetAllDirectories().Where(d => d.Size <= 100000).Sum(d => d.Size),
            95437);

    [Fact]
    public override async Task Part1_Actual()
        => await Invoke(
            Part.One,
            DataType.Actual,
            fs => fs.GetAllDirectories().Where(d => d.Size <= 100000).Sum(d => d.Size),
            1077191);

    [Fact]
    public override async Task Part2_Example()
        => await Invoke(
            Part.Two,
            DataType.Example,
            fs => fs.GetDirectoryDeletionCandidate(30000000).Size,
            24933642);

    [Fact]
    public override async Task Part2_Actual()
        => await Invoke(
            Part.Two,
            DataType.Actual,
            fs => fs.GetDirectoryDeletionCandidate(30000000).Size,
            5649896);

    private async Task Invoke(Part part, DataType dataType, Func<FileSystem, int> sut, int? expected = null)
    {
        var data = await GetData(dataType);
        var commands = Parser.Parse(data);
        var fs = FileSystem.MapFromCommands(commands);
        var result = sut.Invoke(fs);

        WriteResult(part, result);

        if (expected.HasValue)
        {
            result.Should().Be(expected);
        }
    }

    private static class Parser
    {
        public static List<FileSystemCommand> Parse(string[] data)
        {
            List<FileSystemCommand> commands = new();
            foreach (var line in data)
            {
                if (FileSystemCommand.IsCommand(line))
                {
                    commands.Add(FileSystemCommand.Parse(line));
                }
                else
                {
                    commands.Last().Output.Add(FileSystemStat.Parse(line));
                }
            }
            return commands;
        }
    }

    private class FileSystem
    {
        public static int TotalSize => 70000000;
        public int TotalSpaceConsumed => _root.Size;
        public int TotalSpaceAvailable => TotalSize - TotalSpaceConsumed;
        public static readonly string Separator = @"\";
        private readonly List<string> _currentDirectoryParts = new() { "/" };
        private string CurrentDirectoryString => string.Join(Separator, _currentDirectoryParts);

        private readonly DirectoryStat _root = new("/", "/");

        private DirectoryStat CurrentDirectory => _currentDirectoryParts.Aggregate(
            (DirectoryStat)null,
            (candidate, next) => (DirectoryStat)(candidate == null
                ? _root.Path == next ? _root : null
                : candidate.First(c => c.StatType == FileSystemStatType.Directory && c.Path == BuildPath(candidate.Path, next))));

        public static FileSystem MapFromCommands(List<FileSystemCommand> commands)
        {
            var fs = new FileSystem();
            commands.ForEach(fs.ExecuteCommand);
            return fs;
        }

        public void ExecuteCommand(FileSystemCommand command)
        {
            if (command is ChangeDirectoryCommand cd)
            {
                ExecuteCommand(cd);
            }
            else if (command is ListContentsCommand ls)
            {
                ExecuteCommand(ls);
            }
        }

        public void ExecuteCommand(ChangeDirectoryCommand command)
        {
            if (command.Direction == NavigationDirection.Forward)
            {
                NavigateForward(command.TargetDirectory);
            }
            else
            {
                NavigateBack();
            }
        }

        public void ExecuteCommand(ListContentsCommand command)
        {
            command.Output.ForEach(stat => stat.Path = BuildPath(CurrentDirectoryString, stat.Name));
            CurrentDirectory.AddFileSystemStats(command.Output);
        }

        public void NavigateBack()
        {
            if (_currentDirectoryParts.Count <= 1)
            {
                throw new InvalidOperationException("Unable to navigate back from root directory");
            }
            _currentDirectoryParts.RemoveAt(_currentDirectoryParts.Count - 1);
        }

        public void NavigateForward(string directory)
        {
            // Since the FileSystem is initialized at the root directory, and the first command
            // is expected to cd into the root directory, we can ignore the first command            
            if (IsRootDirectory(directory) && IsRootDirectory(CurrentDirectoryString)) return;

            var path = BuildPath(CurrentDirectoryString, directory);

            if (!CurrentDirectory.Any(fs => fs.StatType == FileSystemStatType.Directory && fs.Path == path))
            {
                CurrentDirectory.AddFileSystemStat(new DirectoryStat(path, directory));
            }

            _currentDirectoryParts.Add(directory);
        }

        public IEnumerable<DirectoryStat> GetAllDirectories()
        {
            yield return _root;

            foreach (var dir in _root.GetDirectories(true))
            {
                yield return dir;
            }
        }

        public DirectoryStat GetDirectoryDeletionCandidate(int requiredSpace)
            => GetAllDirectories()
                .Where(d => d.Size >= requiredSpace - TotalSpaceAvailable)
                .OrderBy(d => d.Size)
                .First();

        private static string BuildPath(params string[] parts)
            => string.Join(Separator, parts);

        private static bool IsRootDirectory(string directory)
            => directory == "/";
    }

    private interface IFileSystemCommand<TCommand>
    {
        public abstract static string Text { get; }
        public abstract static bool TryParse(string input, out TCommand command);
    }


    private abstract class FileSystemCommand
    {
        public CommandOutput Output { get; } = new CommandOutput();
        public static bool IsCommand(string data) => data.StartsWith("$");

        public static FileSystemCommand Parse(string input)
        {
            if (!IsCommand(input))
                throw new InvalidOperationException($"The specified input {input} is not a file system command");

            if (ChangeDirectoryCommand.TryParse(input, out var cd))
                return cd;

            if (ListContentsCommand.TryParse(input, out var ls))
                return ls;

            throw new InvalidCastException($"Unable to parse the input to a ${typeof(FileSystemCommand).Name}");
        }
    }

    private enum NavigationDirection { Forward, Back };

    private class ChangeDirectoryCommand : FileSystemCommand, IFileSystemCommand<ChangeDirectoryCommand>
    {
        public static string Text => "cd";

        public string TargetDirectory { get; }
        public NavigationDirection Direction { get; }

        public ChangeDirectoryCommand(string targetDirectory)
        {
            TargetDirectory = targetDirectory;
            Direction = TargetDirectory == ".." ? NavigationDirection.Back : NavigationDirection.Forward;
        }

        public static bool TryParse(string input, out ChangeDirectoryCommand command)
        {
            command = null;
            if (!IsCommand(input)) return false;

            var parts = input.Split();
            if (parts.Length != 3) return false;
            if (parts.Skip(1).First() != Text) return false;

            command = new ChangeDirectoryCommand(parts.Last());
            return true;
        }
    }

    private class ListContentsCommand : FileSystemCommand, IFileSystemCommand<ListContentsCommand>
    {
        public static string Text => "ls";
        public static bool TryParse(string input, out ListContentsCommand command)
        {
            command = null;
            if (!IsCommand(input)) return false;

            var parts = input.Split();
            if (parts.Length != 2) return false;
            if (parts.Skip(1).First() != Text) return false;

            command = new ListContentsCommand();
            return true;
        }

        public IEnumerable<FileStat> GetOutputFileStats()
            => Output.Where(o => o.StatType == FileSystemStatType.File).Cast<FileStat>();
    }

    private enum FileSystemStatType { File, Directory };

    private abstract class FileSystemStat
    {
        public string Name { get; protected set; }
        public string Path { get; set; }
        public abstract int Size { get; }
        public abstract FileSystemStatType StatType { get; }

        public static FileSystemStat Parse(string input)
        {
            if (DirectoryStat.TryParse(input, out var dirStat))
                return dirStat;
            if (FileStat.TryParse(input, out var fileStat))
                return fileStat;

            throw new InvalidCastException($"Unable to parse the input to a ${typeof(FileSystemStat).Name}");
        }
    }

    private interface IFileSystemStatParser<TFileSystemStat> where TFileSystemStat : FileSystemStat
    {
        public abstract static bool TryParse(string input, out TFileSystemStat stat);
    }

    private class FileStat : FileSystemStat, IFileSystemStatParser<FileStat>
    {
        public override int Size { get; }
        public override FileSystemStatType StatType => FileSystemStatType.File;

        public FileStat(string name, int size)
        {
            Name = name;
            Size = size;
        }

        public static bool TryParse(string input, out FileStat stat)
        {
            stat = null;
            var parts = input.Split();
            if (parts.Length == 2 && int.TryParse(parts.First(), out var size))
            {
                stat = new FileStat(parts.Last(), size);
                return true;
            }
            return false;
        }
    }

    private class DirectoryStat : FileSystemStat, IFileSystemStatParser<DirectoryStat>, IEnumerable<FileSystemStat>
    {
        public static readonly string Prefix = "dir";
        public override int Size => _contents.Sum(c => c.Size);
        public override FileSystemStatType StatType => FileSystemStatType.Directory;
        private readonly List<FileSystemStat> _contents = new();

        public DirectoryStat(string name)
        {
            Name = name;
        }

        public DirectoryStat(string path, string name) : this(name)
        {
            Path = path;
        }

        public static bool TryParse(string input, out DirectoryStat stat)
        {
            stat = null;
            var parts = input.Split();
            if (parts.Length == 2 && parts.First() == Prefix)
            {
                stat = new DirectoryStat(parts.Last());
                return true;
            }
            return false;
        }
        public void AddFileSystemStat(FileSystemStat fsStat)
        {
            _contents.Add(fsStat);
        }
        public void AddFileSystemStats(IEnumerable<FileSystemStat> fsStats)
        {
            _contents.AddRange(fsStats);
        }

        public IEnumerable<DirectoryStat> GetDirectories(bool includeSubdirectories)
        {
            foreach (var item in _contents)
            {
                if (item is DirectoryStat dirStat)
                {
                    yield return dirStat;

                    foreach (var subSir in dirStat.GetDirectories(includeSubdirectories))
                    {
                        yield return subSir;
                    }
                }
            }
        }

        public IEnumerator<FileSystemStat> GetEnumerator()
            => _contents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private class CommandOutput : List<FileSystemStat>
    {
        public CommandOutput() { }
    }
}