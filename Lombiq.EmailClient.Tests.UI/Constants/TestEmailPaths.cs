using System;
using System.IO;

namespace Lombiq.EmailClient.Tests.UI.Constants;

public static class TestEmailPaths
{
    private static readonly string BasePath = Path.Combine(Environment.CurrentDirectory, "TestEmails");

    public static readonly string SampleImportant1FileName = "sample_important_1.eml";
    public static readonly string SampleImportant2FileName = "sample_important_2.eml";
    public static readonly string SampleNotImportantFileName = "sample_not_important.eml";

    public static readonly string SampleImportant1Path = Path.Combine(BasePath, SampleImportant1FileName);
    public static readonly string SampleImportant2Path = Path.Combine(BasePath, SampleImportant2FileName);
    public static readonly string SampleNotImportantPath = Path.Combine(BasePath, SampleNotImportantFileName);

    public static readonly string[] SampleEmailPaths =
    [
        SampleImportant1Path,
        SampleImportant2Path,
        SampleNotImportantPath
    ];
}
