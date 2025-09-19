// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace BuildPipeline;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using System.Globalization;
using System.Xml;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

internal partial class BuildPipeline
{
    private Target BuildTests => target =>
        target.DependsOn(Restore)
              .Executes(() =>
              {
                  TestsDirectory.GlobFiles("**/*.csproj")
                                .ForEach(testProject =>
                                {
                                    DotNetBuild(s => s
                                                     .SetProjectFile(testProject)
                                                     .SetConfiguration(TargetBuildConfiguration)
                                                     .SetProcessAdditionalArguments("/nowarn:CS1591;CS1998;CS4014")
                                                     .EnableNoRestore());
                                });
              });

    private Target Test => target =>
        target.DependsOn(BuildTests, RestoreTools)
              .Executes(() =>
              {
                  DotNetTest(s => s
                                  .SetConfiguration(TargetBuildConfiguration)
                                  .EnableNoBuild()
                                  .EnableNoRestore()
                                  .SetDataCollector("XPlat Code Coverage")
                                  .SetResultsDirectory(CoverageDirectory)
                                  .CombineWith(
                                      TestsDirectory.GlobFiles("**/*.Tests.csproj"),
                                      (settings, path) =>
                                      {
                                          var testOutput = ArtifactsDirectory / (Path.GetFileNameWithoutExtension(path) + ".xml");
                                          return settings.SetProjectFile(path)
                                                         .SetLoggers($"junit;LogFilePath={testOutput}");
                                      }));

                  DotNet($"coverage merge {CoverageDirectory}/**/coverage.cobertura.xml -f cobertura -o {MergedCoverageResultsFile}");

                  ReportTestCountAndCoverage();
              });

    private void ReportTestCountAndCoverage()
    {
        var totalTestCount = 0;
        var totalFailureCount = 0;

        var xmlReaderSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        foreach (var testResult in ArtifactsDirectory.GlobFiles("*.Tests.xml"))
        {
            using var reader = XmlReader.Create(testResult, xmlReaderSettings);
            reader.Read();
            reader.Read();

            if (reader is not { NodeType: XmlNodeType.Element, Name: "testsuites" } ||
                !Int32.TryParse(reader.GetAttribute("tests"), out var testCount) ||
                !Int32.TryParse(reader.GetAttribute("failures"), out var failureCount))
            {
                throw new InvalidDataException($"Malformed test result file: {testResult}");
            }

            totalTestCount += testCount;
            totalFailureCount += failureCount;
        }

        using var coverageReader = XmlReader.Create(MergedCoverageResultsFile, xmlReaderSettings);
        coverageReader.Read();
        coverageReader.Read();

        if (coverageReader is not { NodeType: XmlNodeType.Element, Name: "coverage" } ||
            !Double.TryParse(coverageReader.GetAttribute("line-rate"), NumberStyles.Number, CultureInfo.InvariantCulture, out var lineRate))
        {
            throw new InvalidDataException($"Malformed test coverage result file: {MergedCoverageResultsFile}");
        }

        var coverage = (lineRate * 100).ToString("N2");
        Console.WriteLine($"CODE_COVERAGE={coverage}");

        ReportSummary(c => c.AddPair("Total", totalTestCount)
                            .AddPair("Failed", totalFailureCount)
                            .AddPair("Coverage", $"{coverage}%"));
    }
}
