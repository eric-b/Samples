<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#nullable enable

Task Main()
{
	return DockerScript.Run(DockerScript.Options, this.QueryCancelToken);
}

internal static class DockerScript
{
	private const string GlobalAppName = "demo";
	private static readonly string ScriptRootPath = Path.GetDirectoryName(Util.CurrentQueryPath)!;
	private static readonly string OutputDirectoryPath = Path.Combine(ScriptRootPath, GlobalAppName);
	private static readonly string DockerfileCopyDirectoryPath = Path.Combine(OutputDirectoryPath, "Dockerfiles");
	private static readonly string AppSourcePath = Path.Combine(Path.GetDirectoryName(ScriptRootPath)!, "src");
	public static readonly DockerBuildScriptOptions Options = new DockerBuildScriptOptions(
		GlobalAppName,
		AppSourcePath,
		DockerfileCopyDirectoryPath,
		OutputDirectoryPath);

	public static Task<DockerfileTransformed[]> Run(DockerBuildScriptOptions options, CancellationToken cancellationToken)
	{
		var helper = new Script(options);
		helper.OnProgress += (sender, args) => Util.Progress = args.ProgressPercent;
		return helper.Run(cancellationToken);
	}
	
	public sealed record DockerfileTransformed(string OriginalDockerfilePath, string TransformedDockerfilePath);

	public sealed record DockerBuildScriptOptions(string GlobalAppName,
									  string AppSourcePath,
									  string DockerfileCopyDirectoryPath,
									  string OutputDirectoryPath)
	{
		public string ImageNamePrefix { get; set; } = $"{GlobalAppName}-";

		public string ImageTag { get; set; } = $"{DateTime.Now:yyyyMMdd-HHmm}";

		public string[] SectionMarkersToExclude { get; set; } = ["SONAR"];

		public bool DisableDockerBuild { get; set; }
	}

	public static string GetImageNameFromDockerfilePath(string originalDockerfilePath)
	{
		return Path.GetFileName(originalDockerfilePath)!.Replace("Dockerfile-", string.Empty, ignoreCase: true, null);
	}

	private class Script(DockerBuildScriptOptions options)
	{
		public class ProgressEventArgs(int progress) : EventArgs
		{
			public int ProgressPercent { get; } = progress;
		}

		public event EventHandler<ProgressEventArgs>? OnProgress;

		public async Task<DockerfileTransformed[]> Run(CancellationToken cancellationToken)
		{
			var dockerfilesTransformed = new List<DockerfileTransformed>();
			var dockerfiles = Directory.GetFiles(options.AppSourcePath, "Dockerfile*", SearchOption.AllDirectories);
			if (dockerfiles.Length == 0)
				throw new ArgumentException($"No Dockerfile found in {options.AppSourcePath}.");

			foreach (var originalDockerfilePath in dockerfiles)
			{
				var transformedCopy = await TransformDockerfile(originalDockerfilePath, cancellationToken);
				if (originalDockerfilePath == transformedCopy)
				{
					continue;
				}
				dockerfilesTransformed.Add(new DockerfileTransformed(originalDockerfilePath, transformedCopy));
			}

			if (options.DisableDockerBuild)
				return dockerfilesTransformed.ToArray();

			var currentIndex = 0;
			foreach (var item in dockerfilesTransformed)
			{
				OnProgress?.Invoke(this, new ProgressEventArgs((int)Math.Round(100 * ((double)(currentIndex++) + 1) / dockerfilesTransformed.Count)));
				await BuildImage(item.OriginalDockerfilePath, item.TransformedDockerfilePath, cancellationToken);
			}

			return dockerfilesTransformed.ToArray();
		}

		async Task<string> TransformDockerfile(string dockerfilePath, CancellationToken cancellationToken)
		{
			var imgName = GetImageNameFromDockerfilePath(dockerfilePath);
			var outputPath = Path.Combine(options.DockerfileCopyDirectoryPath, "Dockerfile-" + imgName + "-local" + Path.GetExtension(dockerfilePath));
			var lines = await File.ReadAllLinesAsync(dockerfilePath, cancellationToken);
			var sb = new StringBuilder();
			for (int i = 0; i < lines.Length; i++)
			{
				var currentLine = lines[i];

				string? sectionMarker;
				if (currentLine.StartsWith("###") && currentLine.EndsWith("###") && (sectionMarker = Array.Find(options.SectionMarkersToExclude, t => currentLine.Contains(t, StringComparison.OrdinalIgnoreCase))) != null)
				{
					int indexStartBlock = i + 1;
					while (i + 1 < lines.Length)
					{
						currentLine = lines[++i];
						if (currentLine.StartsWith("###") && currentLine.EndsWith("###") && currentLine.Contains(sectionMarker, StringComparison.OrdinalIgnoreCase))
						{
							currentLine = lines[++i];
							break;
						}
					}
					int indexEndBlock = i;
					sb.AppendLine($"#####################################################");
					sb.AppendLine($"# Skipped section [{indexStartBlock}:{indexEndBlock}]");
					sb.AppendLine($"#####################################################");
				}

				if (currentLine.StartsWith("RUN dotnet test") ||
					(currentLine.StartsWith("RUN dotnet restore ") && currentLine.EndsWith(".Tests")) ||
					(currentLine.StartsWith("RUN dotnet build ") && currentLine.Contains(".Tests ")))
				{
					do
					{
						sb.Append("# Skipped: ");
						sb.AppendLine(currentLine);
						currentLine = lines[++i];
					} while (currentLine.EndsWith(@"\"));
				}


				sb.AppendLine(currentLine);
			}

			Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
			await File.WriteAllTextAsync(outputPath, sb.ToString(), cancellationToken);
			Console.WriteLine(outputPath);
			return outputPath;
		}

		async Task BuildImage(string originalDockerfilePath, string transformedDockerfilePath, CancellationToken cancellationToken)
		{
			var imgName = GetImageNameFromDockerfilePath(originalDockerfilePath);
			string filename = Path.GetFileNameWithoutExtension(transformedDockerfilePath);
			var tmpDockerfilePath = Path.Combine(Path.GetDirectoryName(originalDockerfilePath)!, filename);
			if (tmpDockerfilePath == originalDockerfilePath)
				return;

			var originalDockerfileDirectoryPath = Path.GetDirectoryName(originalDockerfilePath)!;

			try
			{
				var imageName = options.ImageNamePrefix + imgName.ToLower();
				var dockerFileArg = Path.GetRelativePath(originalDockerfileDirectoryPath, transformedDockerfilePath);
				var psi = new ProcessStartInfo("docker", $"build  -t {imageName}:latest -t {imageName}:{options.ImageTag} -f {dockerFileArg} .");

				psi.WorkingDirectory = originalDockerfileDirectoryPath;
				psi.RedirectStandardError = true;
				psi.RedirectStandardOutput = true;
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;

				Console.WriteLine(psi.WorkingDirectory);
				Console.WriteLine($"{psi.FileName} {psi.Arguments}");
				using var p = new Process();
				p.StartInfo = psi;
				p.OutputDataReceived += ProcessOutputHandler;
				p.ErrorDataReceived += ProcessOutputHandler;
				p.Start();
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				await p.WaitForExitAsync(cancellationToken);
				if (p.ExitCode != 0)
				{
					throw new ArgumentException($"Docker build failed: {transformedDockerfilePath} - exit code: {p.ExitCode}");
				}

				p.OutputDataReceived -= ProcessOutputHandler;
				p.ErrorDataReceived -= ProcessOutputHandler;
				Console.WriteLine($"Completed build {transformedDockerfilePath}");
			}
			finally
			{
				File.Delete(tmpDockerfilePath);
			}
		}

		static void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			Console.WriteLine(outLine.Data);
		}
	}
}
