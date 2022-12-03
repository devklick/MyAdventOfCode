using System;
using System.IO;
using System.Threading.Tasks;

using Xunit.Abstractions;

namespace MyAdventOfCode.Common;

public enum DataType { Example, Actual }

public abstract class TestBase
{
    protected readonly ITestOutputHelper _output;

    public TestBase(ITestOutputHelper output)
    {
        _output = output;
    }

    protected abstract int DayNo { get; }
    protected abstract int YearNo { get; }
    private static string WantedPath => Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
    private string RootPath => Path.Combine(WantedPath, $"Y{YearNo}", $"D{DayNo:D2}");
    private string DataFilePath => Path.Combine(RootPath, "data.txt");
    private string ExampleDataFilePath => Path.Combine(RootPath, "example.data.txt");
    private string TestName => $"Y{YearNo} D{DayNo:D2}";

    protected virtual async Task<string[]> GetActualData()
        => await File.ReadAllLinesAsync(DataFilePath);

    protected virtual async Task<string[]> GetExampleData()
        => await File.ReadAllLinesAsync(ExampleDataFilePath);

    protected virtual async Task<string[]> GetData(DataType dataType) => dataType switch
    {
        DataType.Example => await GetExampleData(),
        DataType.Actual => await GetActualData(),
        _ => throw new NotImplementedException($"Data type {dataType} not supported"),
    };

    protected void WriteResult<T>(Part part, T result)
        => _output.WriteLine($"{TestName} part {part}: {result}");

    public abstract Task Part1_VerifyExample();
    public abstract Task Part1_Actual();
    public abstract Task Part2_VerifyExample();
    public abstract Task Part2_Actual();
}