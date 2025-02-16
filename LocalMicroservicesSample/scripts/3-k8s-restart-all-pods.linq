<Query Kind="Program">
  <NuGetReference>KubernetesClient</NuGetReference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>k8s</Namespace>
  <Namespace>k8s.Models</Namespace>
</Query>

#nullable enable
#load "1-docker-build-images.linq"
#load "2-k8s-generate-manifests.linq"

const ScriptAction programAction = ScriptAction.RestartAll;
enum ScriptAction
{
	None,
	StopAll,
	RestartAll,
	OpenTofuDestroyAll,
	OpenTofuInit
}

sealed record OpenTofuProject(string name);

static class DeployScriptConstants
{
	internal const bool DisableManifestGeneration = false;
	internal static readonly string OpentofuBinary = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "OpenTofu", "tofu.exe");

	internal static readonly OpenTofuProject DevDepsProject = new("dev-dependencies");
	internal static readonly OpenTofuProject DemoProject = new(ManifestsScript.Options.GlobalAppName);

	internal static readonly string DockerfileOriginalDirectoryPath = DockerScript.Options.AppSourcePath;
	internal static readonly string OutputDirectoryPath = DockerScript.Options.OutputDirectoryPath;

	internal static readonly string DemoK8sNamespace = ManifestsScript.Options.GlobalAppName;
}


sealed record OpenTofuInfo(string BinaryPath, string WorkspacePath, string BackendConfigPath, string SecretVariablesPath)
{
	public static OpenTofuInfo Create(string binaryPath, OpenTofuProject project)
	{
		var workspacePath = Path.Combine(Path.GetDirectoryName(DeployScriptConstants.OutputDirectoryPath)!, "3-opentofu", project.name);
		var backendFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenTofu", project.name);
		var backendValuesConfigPath = Path.Combine(backendFolderPath, "state.config");
		var secretVariablesPath = Path.Combine(backendFolderPath, "terraform.tfvars");

		if (!File.Exists(backendValuesConfigPath))
			throw new System.IO.FileNotFoundException($"Path not found: {backendValuesConfigPath}. This file must contain state configuration.");
		if (!File.Exists(secretVariablesPath))
			throw new System.IO.FileNotFoundException($"Path not found: {secretVariablesPath}. This file must contain some secrets we do not store near other opentofu files.");

		return new OpenTofuInfo(binaryPath, workspacePath, backendValuesConfigPath, secretVariablesPath);
	}
}

Task Main()
{
	return new DeployProgram().Run(this.QueryCancelToken);
}

internal class DeployProgram
{
	internal async Task Run(CancellationToken cancellationToken)
	{
		switch (programAction)
		{
			case ScriptAction.RestartAll:
				if (!DeployScriptConstants.DisableManifestGeneration)
					await ManifestsScript.Run(cancellationToken);
				await OpenTofuApply(DeployScriptConstants.DevDepsProject, cancellationToken);
				await OpenTofuApply(DeployScriptConstants.DemoProject, cancellationToken);
				await RestartAllPods(cancellationToken);
				break;
			case ScriptAction.StopAll:
				if (!DeployScriptConstants.DisableManifestGeneration)
					await ManifestsScript.Run(cancellationToken);
				await StopAllPods(cancellationToken);
				break;
			case ScriptAction.OpenTofuDestroyAll:
				await OpenTofuDestroy(DeployScriptConstants.DemoProject, cancellationToken);
				break;
			case ScriptAction.OpenTofuInit:
				await OpenTofuInit(DeployScriptConstants.DevDepsProject, cancellationToken);
				await OpenTofuInit(DeployScriptConstants.DemoProject, cancellationToken);
				break;
			default:
				break;
		}
	}

	async Task RunCommand(ProcessStartInfo psi, CancellationToken cancellationToken)
	{
		
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

		p.OutputDataReceived -= ProcessOutputHandler;
		p.ErrorDataReceived -= ProcessOutputHandler;

		if (p.ExitCode != 0)
		{
			throw new ArgumentException($"{psi.FileName} {psi.Arguments}: exit code = {p.ExitCode}");
		}
	}

	async Task OpenTofuApply(OpenTofuProject project, CancellationToken cancellationToken)
	{
		var paths = OpenTofuInfo.Create(DeployScriptConstants.OpentofuBinary, project);
		var psi = new ProcessStartInfo(paths.BinaryPath, $"apply -auto-approve -var-file \"{paths.SecretVariablesPath}\"");
		psi.WorkingDirectory = paths.WorkspacePath;
		await RunCommand(psi, cancellationToken);
	}

	async Task OpenTofuInit(OpenTofuProject project, CancellationToken cancellationToken)
	{
		var paths = OpenTofuInfo.Create(DeployScriptConstants.OpentofuBinary, project);
		var psi = new ProcessStartInfo(paths.BinaryPath, $"init -backend-config \"{paths.BackendConfigPath}\"");
		psi.WorkingDirectory = paths.WorkspacePath;
		await RunCommand(psi, cancellationToken);
	}

	async Task OpenTofuDestroy(OpenTofuProject project, CancellationToken cancellationToken)
	{
		var paths = OpenTofuInfo.Create(DeployScriptConstants.OpentofuBinary, project);

		var psi = new ProcessStartInfo(paths.BinaryPath, $"destroy -auto-approve -var-file \"{paths.SecretVariablesPath}\"");
		psi.WorkingDirectory = paths.WorkspacePath;
		await RunCommand(psi, cancellationToken);
	}

	async Task RestartAllPods(CancellationToken cancellationToken)
	{
		string[] statefullSetNames = await GetAllStatefullSetNames(cancellationToken);

		using var client = CreateK8sClient();

		bool needToWait = await ScaleTo(0, client, statefullSetNames, cancellationToken);
		if (needToWait)
		{
			await WaitScaling(0, client, statefullSetNames, cancellationToken);
		}

		needToWait = await ScaleTo(1, client, statefullSetNames, cancellationToken);
		if (needToWait)
		{
			await WaitScaling(1, client, statefullSetNames, cancellationToken);
		}
	}

	async Task StopAllPods(CancellationToken cancellationToken)
	{
		string[] statefullSetNames = await GetAllStatefullSetNames(cancellationToken);

		using var client = CreateK8sClient();

		bool needToWait = await ScaleTo(0, client, statefullSetNames, cancellationToken);
	}

	async Task WaitScaling(int targetReplicaCount, Kubernetes client, string[] statefullSetNames, CancellationToken cancellationToken)
	{
		foreach (string name in statefullSetNames)
		{
			try
			{
				var scaleResult = await client.ReadNamespacedStatefulSetScaleAsync(name, DeployScriptConstants.DemoK8sNamespace, cancellationToken: cancellationToken);
				while (scaleResult.Status.Replicas != targetReplicaCount)
				{
					Console.WriteLine($"Waiting for replica count {targetReplicaCount}...");
					await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
					scaleResult = await client.ReadNamespacedStatefulSetScaleAsync(name, DeployScriptConstants.DemoK8sNamespace, cancellationToken: cancellationToken);
				}

				var resourceStatus = await client.ReadNamespacedStatefulSetStatusAsync(name, DeployScriptConstants.DemoK8sNamespace, cancellationToken: cancellationToken);
				while (resourceStatus.Status.ReadyReplicas != resourceStatus.Status.CurrentReplicas)
				{
					Console.WriteLine($"Waiting for ready state...");
					await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
					resourceStatus = await client.ReadNamespacedStatefulSetStatusAsync(name, DeployScriptConstants.DemoK8sNamespace, cancellationToken: cancellationToken);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to status of probe {name}.", ex);
			}
		}
	}

	async Task<bool> ScaleTo(int newReplicaCount, Kubernetes client, string[] statefullSetNames, CancellationToken cancellationToken)
	{
		string pathString = @"{ ""spec"": { ""replicas"": " + newReplicaCount.ToString() + @" } }";
		V1Patch patch = new(pathString, V1Patch.PatchType.MergePatch);
		bool needToWait = false;
		foreach (string name in statefullSetNames)
		{
			try
			{
				var result = await client.PatchNamespacedStatefulSetScaleAsync(patch, name, DeployScriptConstants.DemoK8sNamespace, cancellationToken: cancellationToken);
				needToWait |= result.Status.Replicas != newReplicaCount;
				Console.WriteLine($"{name}: scaling to {newReplicaCount} (from current {result.Status.Replicas})...");
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to patch {name}.", ex);
			}
		}

		return needToWait;
	}

	async Task<string[]> GetAllStatefullSetNames(CancellationToken cancellationToken)
	{
		var result = new List<string>();
		var yamlFiles = Directory.GetFiles(DeployScriptConstants.OutputDirectoryPath, "*.yaml", SearchOption.AllDirectories);
		foreach (var item in yamlFiles)
		{
			var name = await GetStatefullSetNameIfAny(item, cancellationToken);
			if (name != null)
				result.Add(name);
		}

		return result.ToArray();
	}

	private async Task<string?> GetStatefullSetNameIfAny(string yamlManifestPath, CancellationToken cancellationToken)
	{
		var yamlLines = await File.ReadAllLinesAsync(yamlManifestPath, cancellationToken);
		if (!yamlLines.Contains("kind: StatefulSet"))
			return null;

		var indexMetadata = Array.FindIndex(yamlLines, t => t == "metadata:");
		if (indexMetadata != -1 && yamlLines.Length > indexMetadata + 1)
		{
			var nameLine = yamlLines[indexMetadata + 1];
			var indexName = nameLine.IndexOf("  name: ");
			if (indexName != -1)
			{
				string statefullSetName = nameLine.Substring(indexName + "  name: ".Length).Trim();
				return statefullSetName;
			}
		}

		return null;
	}

	static Kubernetes CreateK8sClient()
	{
		KubernetesClientConfiguration config;
		var kubeconfig = Environment.GetEnvironmentVariable("KUBECONFIG");
		if (kubeconfig != null)
		{
			config = KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeconfig);
		}
		else
		{
			config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
		}
		var client = new Kubernetes(config);
		return client;
	}

	static void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
	{
		Console.WriteLine(outLine.Data);
	}
}