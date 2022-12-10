using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit.Abstractions;

namespace MyAdventOfCode.Common;

public enum DataType { Example, Actual }

public abstract class TestBase
{
    protected readonly ITestOutputHelper _output;
    private readonly int _dayNo;
    private readonly int _yearNo;
    private string YearString => $"Y{_yearNo}";
    private string DayString => $"D{_dayNo:D2}";
    private static string RootPath => Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
    private string FolderPath => Path.Combine(RootPath, YearString, DayString);
    private string DataFilePath => Path.Combine(FolderPath, "data.txt");
    private string ExampleDataFilePath => Path.Combine(FolderPath, "example.data.txt");
    private string TestName => $"{YearString} {DayString}";

    public TestBase(ITestOutputHelper output)
    {
        _output = output;
        var namespaceParts = GetType().Namespace.Split(".");
        _dayNo = int.Parse(new string(namespaceParts.Last().Where(char.IsDigit).ToArray()));
        _yearNo = int.Parse(new string(namespaceParts.Reverse().Skip(1).First().Where(char.IsDigit).ToArray()));
        Console.SetOut(new ConsoleWriter(_output));
    }

    protected virtual async Task<string[]> GetActualData()
        => await GetData(DataFilePath);

    protected virtual async Task<string[]> GetExampleData()
        => await GetData(ExampleDataFilePath);

    protected virtual async Task<string[]> GetData(DataType dataType) => dataType switch
    {
        DataType.Example => await GetExampleData(),
        DataType.Actual => await GetActualData(),
        _ => throw new NotImplementedException($"Data type {dataType} not supported"),
    };

    protected void WriteResult<T>(Part part, T result)
        => _output.WriteLine($"{TestName} Part {part}: {result}");

    public abstract Task Part1_Example();
    public abstract Task Part1_Actual();
    public abstract Task Part2_Example();
    public abstract Task Part2_Actual();

    private static async Task<string[]> GetData(string path)
    {
        var data = await File.ReadAllLinesAsync(path);
        if (string.IsNullOrEmpty(data.Last()))
        {
            data = data.Take(data.Length - 1).ToArray();
        }
        return data;
    }
}