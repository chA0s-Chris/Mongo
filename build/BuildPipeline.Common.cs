﻿// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace BuildPipeline;

using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

internal partial class BuildPipeline
{
    [Parameter]
    public String ReleaseVersion { get; set; } = "0.1.0-dev";

    private AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    private String AssemblyVersion { get; set; } = String.Empty;

    private Target Clean => target =>
        target.Before(Restore, RestoreTools, Build, /*Pack,*/ BuildTests, Test)
              .Executes(() =>
              {
                  DotNetTasks.DotNetClean(x => x.SetConfiguration(TargetBuildConfiguration)
                                                .EnableContinuousIntegrationBuild()
                                                .DisableProcessOutputLogging());

                  ArtifactsDirectory.CreateOrCleanDirectory();
                  PublishDirectory.CreateOrCleanDirectory();
              });

    private AbsolutePath CoverageDirectory => ArtifactsDirectory / "test-coverage";

    [GitRepository]
    private GitRepository GitRepository { get; init; } = null!;

    private AbsolutePath MergedCoverageResultsFile => CoverageDirectory / "coverage.cobertura.merged.xml";

    [Parameter(Name = "NUGET_PACKAGES_DIRECTORY")]
    private AbsolutePath PackagesDirectory { get; set; } = RootDirectory / ".nuget";

    private AbsolutePath PublishDirectory => RootDirectory / "publish";

    private String SemanticVersion { get; set; } = String.Empty;

    [Solution]
    private Solution Solution { get; set; } = null!;

    [Parameter]
    private String TargetBuildConfiguration { get; set; } = "Release";

    private AbsolutePath TestsDirectory => RootDirectory / "tests";
}
