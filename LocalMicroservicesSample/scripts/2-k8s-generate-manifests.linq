<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#nullable enable
#load "1-docker-build-images.linq"

Task Main() => ManifestsScript.Run(this.QueryCancelToken);

internal static class ManifestsScript
{
	private static readonly string ScriptRootPath = Path.GetDirectoryName(Util.CurrentQueryPath)!;
	private static readonly string DotEnvFilePath = Path.Combine(ScriptRootPath, ".env");
	
	public static readonly GenerateK8sManifestsScriptOptions Options = new GenerateK8sManifestsScriptOptions(
		DockerScript.Options.GlobalAppName,
		new K8sTemplates(Path.Combine(ScriptRootPath, "2-templates", "k8s")),
		DotEnvFilePath,
		DockerScript.Options.OutputDirectoryPath);
		
	public static async Task Run(CancellationToken cancellationToken)
	{
		var dockerBuildOptions = DockerScript.Options with { DisableDockerBuild = true };
		DockerScript.DockerfileTransformed[] dockerfilesTransformed = await DockerScript.Run(dockerBuildOptions, cancellationToken);

		var generateManifests = new Script(Options);
		await generateManifests.Run(dockerfilesTransformed, cancellationToken);
	}

	public class GenerateK8sManifestsScriptOptions(string globalAppName, K8sTemplates templates, string dotEnvFilePath, string outputDirectoryPath)
	{
		public string GlobalAppName { get; } = globalAppName;

		public string OtelEnv { get; set; } = "local";

		public string DotEnvFilePath { get; } = dotEnvFilePath;

		public string OutputDirectoryPath { get; } = outputDirectoryPath;

		public K8sTemplates K8sTemplates { get; } = templates;

		public string DefaultKestrelUrl { get; set; } = "http://*:8080";

		public string DefaultAzuriteUrl { get; set; } = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite-service:10000/devstoreaccount1;";

		public string DefaultServiceBusUrl { get; set; } = "Endpoint=sb://azure-sb-emulator-service;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
	}

	public class K8sTemplates(string basePath)
	{
		public string Namespace { get; } = Path.Combine(basePath, "Namespace.yaml");

		public string ClusterIpService { get; } = Path.Combine(basePath, "ClusterIpService.yaml");

		public string StatefulSet { get; } = Path.Combine(basePath, "StatefulSet.yaml");

		public string AppSettingsConfigMap { get; } = Path.Combine(basePath, "AppSettingsConfigMap.yaml");

		public string Secrets { get; } = Path.Combine(basePath, "Secrets.yaml");

		public string OtelConfigMap { get; } = Path.Combine(basePath, "OtelConfigMap.yaml");
	}

	private class Script(GenerateK8sManifestsScriptOptions options)
	{
		sealed record HostInfo(string AppName, DirectoryInfo SourceProject);

		sealed record SecretName(string VariableName, string StringDataKey);

		class JsonHelper
		{
			public static Dictionary<string, string> MapToFlatAppSettings(JObject? appSettings)
			{
				if (appSettings is null)
					return new Dictionary<string, string>();

				IEnumerable<JToken> appSettingsJTokens = appSettings.Descendants().Where(p => !p.HasValues);
				Dictionary<string, string> appSettingsFlatDic = appSettingsJTokens.Aggregate(new Dictionary<string, string>(), (properties, jToken) =>
				{
					properties.Add(jToken.Path, jToken.ToString());
					return properties;
				});
				return appSettingsFlatDic;
			}
		}

		class TempleteRenderer(K8sTemplates templates)
		{
			public Task<string> RenderNamespaceTemplate(string k8sNamespace, CancellationToken cancellationToken)
			{
				return File.ReadAllTextAsync(templates.Namespace, cancellationToken);
			}

			public async Task<string> RenderOtelConfigMapTemplate(string appName, string otelEnv, CancellationToken cancellationToken)
			{
				var template = await File.ReadAllTextAsync(templates.OtelConfigMap, cancellationToken);
				var result = template
					.Replace("{appName}", appName)
					.Replace("{env}", otelEnv);

				return result;
			}

			public async Task<string> RenderStatefulSetTemplate(string appName,
														 string otelEnv,
														 string? dbInitContainer,
														 string? dbEnvVariables,
														 CancellationToken cancellationToken)
			{
				var template = await File.ReadAllTextAsync(templates.StatefulSet, cancellationToken);
				var result = template
					.Replace("{appName}", appName)
					.Replace("{env}", otelEnv)
					.Replace("{dbInitContainer}", dbInitContainer)
					.Replace("{dbEnvVariables}", dbEnvVariables);

				return result;
			}

			public async Task<string> RenderClusterIpServiceTemplate(string appName, int portNumber, CancellationToken cancellationToken)
			{
				var template = await File.ReadAllTextAsync(templates.ClusterIpService, cancellationToken);
				var result = template
					.Replace("{appName}", appName)
					.Replace("{portNumber}", portNumber.ToString());

				return result;
			}

			public async Task<string> RenderAppSettingsConfigMapTemplate(string appName, JObject appSettings, CancellationToken cancellationToken)
			{
				string transformedJsonIndentedForConfigmap = string.Join(Environment.NewLine, appSettings.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Select(t => "".PadLeft(4) + t));

				var template = await File.ReadAllTextAsync(templates.AppSettingsConfigMap, cancellationToken);
				var result = template
					.Replace("{appName}", appName)
					.Replace("{appSettings}", transformedJsonIndentedForConfigmap);

				return result;
			}

			public async Task<string> RenderSecretTemplate(string appName, Dictionary<string, string> secrets, CancellationToken cancellationToken)
			{
				string transformedIndentedStringData = string.Join(Environment.NewLine, secrets.Select(t => $"{t.Key}: \"{t.Value}\"").Select(t => "".PadLeft(2) + t));

				var template = await File.ReadAllTextAsync(templates.Secrets, cancellationToken);
				var result = template
					.Replace("{appName}", appName)
					.Replace("{stringData}", transformedIndentedStringData);

				return result;
			}
		}


		sealed class GenerateManifestsContext(HostInfo hostInfo, IReadOnlyDictionary<string, string> dotEnv)
		{
			public HostInfo HostInfo { get; } = hostInfo;

			public Dictionary<SecretName, string> DeploymentSecrets { get; } = new();

			public IReadOnlyDictionary<string, string> DotEnv { get; } = dotEnv;

			public JObject? AppSettingsWrittenInConfigMap { get; set; }

			public async Task<bool> IsWebHost(CancellationToken cancellationToken)
			{
				var controllersPath = Path.Combine(HostInfo.SourceProject.FullName, "Controllers");
				if (Directory.Exists(controllersPath) && Directory.EnumerateFiles(controllersPath, "*.cs", SearchOption.AllDirectories).Any())
				{
					return true;
				}

				var csproj = HostInfo.SourceProject.GetFiles("*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
				if (csproj is null)
					return false;

				var csprojContent = await File.ReadAllTextAsync(csproj.FullName, Encoding.UTF8, cancellationToken);
				return csprojContent.Contains("<Project Sdk=\"Microsoft.NET.Sdk.Web\">");
			}

			public Dictionary<string, string> GetFlatAppSettingsFromConfigMap()
			{
				if (AppSettingsWrittenInConfigMap is null)
					throw new InvalidOperationException("AppSettingsWrittenInConfigMap is required.");

				return JsonHelper.MapToFlatAppSettings(AppSettingsWrittenInConfigMap);
			}

			public async Task<JObject?> GetUserSecretsFromCsProj(CancellationToken cancellationToken)
			{
				var csprojPath = HostInfo.SourceProject.GetFiles("*.csproj", SearchOption.TopDirectoryOnly).SingleOrDefault();
				if (csprojPath != null)
				{
					var csProjXmlContent = await File.ReadAllTextAsync(csprojPath.FullName, cancellationToken);
					string userSecretGuid = Regex.Replace(csProjXmlContent, @"^.+?<UserSecretsId>(.+?)<\/UserSecretsId>.+$", "$1", RegexOptions.Singleline);

					var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @$"Microsoft\UserSecrets\{userSecretGuid}\secrets.json");
					if (!File.Exists(path))
						return null;

					var secretJson = await File.ReadAllTextAsync(path, cancellationToken);
					return JObject.Parse(secretJson);
				}
				return null;
			}
		}
		
		private readonly TempleteRenderer _template = new(options.K8sTemplates);

		public async Task Run(DockerScript.DockerfileTransformed[] dockerfilesTransformed, CancellationToken cancellationToken)
		{
			foreach (var item in dockerfilesTransformed)
			{
				var originalDockerfilePath = item.OriginalDockerfilePath;
				var transformedDockerfilePath = item.TransformedDockerfilePath;
				var hostInfo = await GetServiceInfoFromDockerfile(originalDockerfilePath, transformedDockerfilePath, cancellationToken);
				var dotEnv = await GetEnvDictionary(cancellationToken);
				await GenerateManifests(hostInfo, dotEnv, cancellationToken);
			}
		}

		async Task GenerateManifests(HostInfo hostInfo, IReadOnlyDictionary<string, string> dotEnv, CancellationToken cancellationToken)
		{
			var context = new GenerateManifestsContext(hostInfo, dotEnv);

			await GenerateNamespaceManifest(cancellationToken);

			await GenerateOtelCollectorConfigMap(context, cancellationToken);

			await GenerateAppConfigMap(context, cancellationToken);

			await GenerateAppStatefulSet(context, cancellationToken);

			await GenerateAppSecrets(context, cancellationToken);

			if (await context.IsWebHost(cancellationToken))
			{
				await GenerateServiceManifest(context, cancellationToken);
			}
		}

		static string GetShortServiceNameFromDockerfilepath(string originalDockerfilePath)
			=> DockerScript.GetImageNameFromDockerfilePath(originalDockerfilePath).ToLower();

		async Task<HostInfo> GetServiceInfoFromDockerfile(string originalDockerfilePath, string transformedDockerfilePath, CancellationToken cancellationToken)
		{
			var dockerBuildWorkingDirectory = Path.GetDirectoryName(originalDockerfilePath)!;
			var dockerFileLines = await File.ReadAllLinesAsync(transformedDockerfilePath, cancellationToken);

			string? shortServiceName = null;
			string? sourceDirectoryPath = null;
			for (int i = 0; i < dockerFileLines.Length; i++)
			{
				var currentLine = dockerFileLines[i];

				if (currentLine.StartsWith("COPY ") && currentLine.EndsWith("/source"))
				{
					var d = currentLine["COPY ".Length..];
					d = d.Substring(0, d.Length - "/source".Length).Trim();
					sourceDirectoryPath = Path.Combine(dockerBuildWorkingDirectory, d);
				}
			}

			if (sourceDirectoryPath is null || !Directory.Exists(sourceDirectoryPath))
				throw new ArgumentException($"Cannot infer source project from Dockerfile {transformedDockerfilePath}.");

			shortServiceName = GetShortServiceNameFromDockerfilepath(originalDockerfilePath);
			return new HostInfo(shortServiceName, new DirectoryInfo(sourceDirectoryPath));
		}

		async Task<Dictionary<string, string>> GetEnvDictionary(CancellationToken cancellationToken)
		{
			string[] dotenv = await File.ReadAllLinesAsync(options.DotEnvFilePath, Encoding.UTF8, cancellationToken);
			var dic = new Dictionary<string, string>(dotenv.Length);
			foreach (var line in dotenv)
			{
				var s = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				dic[s[0]] = s[1].Trim('"');
			}
			return dic;
		}

		string GetManifestOutputDirectoryPath(GenerateManifestsContext context)
			=> Path.Combine(options.OutputDirectoryPath, "k8s", context.HostInfo.AppName);

		async Task<string> GenerateServiceManifest(GenerateManifestsContext context, CancellationToken cancellationToken)
		{
			string serviceManifestDirectoryPath = GetManifestOutputDirectoryPath(context);
			Directory.CreateDirectory(serviceManifestDirectoryPath);

			var kestrelPort = GetKestrelPort(context);

			var serviceManifestPath = Path.Combine(serviceManifestDirectoryPath, $"{context.HostInfo.AppName}-service.yaml");
			await File.WriteAllTextAsync(
				serviceManifestPath,
				await _template.RenderClusterIpServiceTemplate(context.HostInfo.AppName, kestrelPort, cancellationToken),
				Encoding.UTF8,
				cancellationToken);

			Console.WriteLine($"Generated {serviceManifestPath}");

			return serviceManifestPath;
		}

		int GetKestrelPort(GenerateManifestsContext context)
		{
			if (!context.DotEnv.TryGetValue("DEFAULT_KESTREL_URL", out var v))
				v = options.DefaultKestrelUrl;

			return int.Parse(v.Split(':', 3)[2]);
		}

		async Task<string> GenerateAppSecrets(GenerateManifestsContext context, CancellationToken cancellationToken)
		{
			var hostInfo = context.HostInfo;
			string serviceDeploymentDirectoryPath = GetManifestOutputDirectoryPath(context);
			var secretsPath = Path.Combine(serviceDeploymentDirectoryPath, $"{hostInfo.AppName}-secrets.yaml");

			var secrets = context.DeploymentSecrets.ToDictionary(kvp => kvp.Key.StringDataKey, kvp => kvp.Value);

			await File.WriteAllTextAsync(
				secretsPath,
				await _template.RenderSecretTemplate(hostInfo.AppName, secrets, cancellationToken),
				Encoding.UTF8,
				cancellationToken);

			Console.WriteLine($"Generated {secretsPath}");

			return secretsPath;
		}

		async Task<string> GenerateAppConfigMap(GenerateManifestsContext context, CancellationToken cancellationToken)
		{
			var hostInfo = context.HostInfo;
			string appSettingsPath = Path.Combine(hostInfo.SourceProject.FullName, "appsettings.json");
			string appSettingsLocalPath = Path.Combine(hostInfo.SourceProject.FullName, "appsettings.Development.json");
			if (!File.Exists(appSettingsPath))
				throw new ArgumentException($"Could not find {appSettingsLocalPath}");

			string serviceDeploymentDirectoryPath = GetManifestOutputDirectoryPath(context);
			var configmapPath = Path.Combine(serviceDeploymentDirectoryPath, $"{hostInfo.AppName}-appsettings-configmap.yaml");
			Directory.CreateDirectory(serviceDeploymentDirectoryPath);

			string settingsJson = await File.ReadAllTextAsync(appSettingsPath, Encoding.UTF8, cancellationToken);

			// We use settingsMergedJObject when we want to force a value whatever it is overriden in Local.
			// Most of the time, we want to override only values from Local settings.
			var settingsJobject = JObject.Parse(settingsJson);
			var settingsMergedJObject = (JObject)settingsJobject.DeepClone();

			JObject? mutableLocalSettingsJobject = null;
			if (File.Exists(appSettingsLocalPath))
			{
				var localSettingsJson = await File.ReadAllTextAsync(appSettingsLocalPath, Encoding.UTF8, cancellationToken);
				mutableLocalSettingsJobject = JObject.Parse(localSettingsJson);
				settingsMergedJObject.Merge(mutableLocalSettingsJobject, new JsonMergeSettings
				{
					MergeArrayHandling = MergeArrayHandling.Merge
				});
			}
			else
			{
				mutableLocalSettingsJobject = new JObject();
			}

			var userSecretsJson = await context.GetUserSecretsFromCsProj(cancellationToken);
			if (userSecretsJson != null)
			{
				settingsMergedJObject.Merge(userSecretsJson, new JsonMergeSettings
				{
					MergeArrayHandling = MergeArrayHandling.Merge
				});
			}

			// If service is exposed, it will be always on port 8080:
			var kestrelPValue = settingsMergedJObject.SelectToken(".Kestrel");
			string defaultKestrelUrl;
			if (!context.DotEnv.TryGetValue("DEFAULT_KESTREL_URL", out defaultKestrelUrl!))
				defaultKestrelUrl = options.DefaultKestrelUrl;

			if (kestrelPValue?.Parent != null)
			{
				mutableLocalSettingsJobject.SelectToken(".Kestrel")?.Parent?.Remove();
				mutableLocalSettingsJobject["Kestrel"] = JObject.FromObject(new { EndPoints = new { Http = new { Url = defaultKestrelUrl } } });
			}
			else if (await context.IsWebHost(cancellationToken))
			{
				mutableLocalSettingsJobject["Kestrel"] = JObject.FromObject(new { EndPoints = new { Http = new { Url = defaultKestrelUrl } } });
			}

			var blobCsPropertyValue = settingsMergedJObject.SelectToken(".BlobStorage.ConnectionString");
			if (blobCsPropertyValue?.Parent != null)
			{
				mutableLocalSettingsJobject.SelectToken(".BlobStorage.ConnectionString")?.Parent?.Remove();

				if (!context.DotEnv.TryGetValue("AZURITE_CONNECTION_STRING", out var azuriteUrl))
					azuriteUrl = options.DefaultAzuriteUrl;
				context.DeploymentSecrets[new SecretName("BlobStorage__ConnectionString", "BlobStorage__ConnectionString")] = azuriteUrl;
			}

			var sbCsPropertyValue = settingsMergedJObject.SelectToken(".ServiceBus.ConnectionString");
			if (sbCsPropertyValue?.Parent != null)
			{
				mutableLocalSettingsJobject.SelectToken(".ServiceBus.ConnectionString")?.Parent?.Remove();

				if (!context.DotEnv.TryGetValue("SERVICE_BUS_CONNECTION_STRING", out var sbUrl))
					sbUrl = options.DefaultServiceBusUrl;
				context.DeploymentSecrets[new SecretName("ServiceBus__ConnectionString", "ServiceBus__ConnectionString")] = sbUrl;
			}

			var primaryServiceDbCsPropertyValue = settingsMergedJObject.SelectToken($".SqlDatabase.ConnectionString");
			if (primaryServiceDbCsPropertyValue?.Parent != null)
			{
				mutableLocalSettingsJobject.SelectToken(".SqlDatabase.ConnectionString")?.Parent?.Remove();

				if (context.DotEnv.TryGetValue("SQL_DATABASE_CONNECTION_STRING", out var cs))
				{
					context.DeploymentSecrets[new SecretName("SqlDatabase__ConnectionString", "SqlDatabase__ConnectionString")] = cs;
				}
			}

			var httpClientJToken = mutableLocalSettingsJobject.SelectToken(".HttpClient");
			if (httpClientJToken is JObject httpClientJObject)
			{
				var kestrelPort = GetKestrelPort(context);
				foreach (var p in httpClientJObject.Properties())
				{
					if (p.Value is JObject clientJObject)
					{
						var baseAddressProp = clientJObject.Property("BaseAddress");
						if (baseAddressProp != null)
						{
							string apiName = p.Name.ToLower();
							baseAddressProp.Value = $"http://{apiName}-service:{kestrelPort}/";
						}
					}
				}
			}

			await File.WriteAllTextAsync(
				configmapPath,
				await _template.RenderAppSettingsConfigMapTemplate(hostInfo.AppName, mutableLocalSettingsJobject, cancellationToken),
				Encoding.UTF8,
				cancellationToken);

			Console.WriteLine($"Generated {configmapPath}");
			context.AppSettingsWrittenInConfigMap = mutableLocalSettingsJobject;

			return configmapPath;
		}

		async Task<string> GenerateNamespaceManifest(CancellationToken cancellationToken)
		{
			var nsManifestPath = Path.Combine(options.OutputDirectoryPath, "k8s", $"{options.GlobalAppName}-namespace.yaml");
			await File.WriteAllTextAsync(
				nsManifestPath,
				await _template.RenderNamespaceTemplate(options.GlobalAppName, cancellationToken),
				Encoding.UTF8,
				cancellationToken);

			Console.WriteLine($"Generated {nsManifestPath}");

			return nsManifestPath;
		}

		async Task GenerateOtelCollectorConfigMap(GenerateManifestsContext context, CancellationToken cancellationToken)
		{
			string serviceDeploymentDirectoryPath = GetManifestOutputDirectoryPath(context);
			var configmapPath = Path.Combine(serviceDeploymentDirectoryPath, $"{context.HostInfo.AppName}-otelcol-configmap.yaml");
			Directory.CreateDirectory(serviceDeploymentDirectoryPath);

			await File.WriteAllTextAsync(
				configmapPath,
				await _template.RenderOtelConfigMapTemplate(context.HostInfo.AppName, options.OtelEnv, cancellationToken),
				Encoding.UTF8, cancellationToken);

			Console.WriteLine($"Generated {configmapPath}");
		}

		async Task<string> GenerateAppStatefulSet(GenerateManifestsContext context, CancellationToken cancellationToken)
		{
			if (context.AppSettingsWrittenInConfigMap is null)
				throw new ArgumentException("AppSettingsWrittenInConfigMap is required at this stage.");

			string serviceDeploymentDirectoryPath = GetManifestOutputDirectoryPath(context);
			var statefulSetPath = Path.Combine(serviceDeploymentDirectoryPath, $"{context.HostInfo.AppName}-statefulSet.yaml");
			Directory.CreateDirectory(serviceDeploymentDirectoryPath);


			var envVariables = new Dictionary<string, string>();
			var initContainers = new Dictionary<string, string>();

			var kestrelPort = GetKestrelPort(context);

			if (!context.DotEnv.TryGetValue("AZURITE_CONNECTION_STRING", out var azuriteUrl))
				azuriteUrl = options.DefaultAzuriteUrl;
			if (!context.DotEnv.TryGetValue("SERVICE_BUS_CONNECTION_STRING", out var sbUrl))
				sbUrl = options.DefaultServiceBusUrl;

			var userSecrets = JsonHelper.MapToFlatAppSettings(await context.GetUserSecretsFromCsProj(cancellationToken));
			foreach (var kvp in userSecrets)
			{
				var variableName = kvp.Key.Replace(".", "__");
				var secretName = new SecretName(variableName, variableName);
				if (!context.DeploymentSecrets.ContainsKey(secretName))
					context.DeploymentSecrets[secretName] = kvp.Value;
			}

			foreach (var kvp in context.GetFlatAppSettingsFromConfigMap())
			{
				var name = kvp.Key;
				var value = kvp.Value;

				if (value.StartsWith("http://") && value.Contains(":" + kestrelPort) && Uri.TryCreate(value, UriKind.Absolute, out var apiUrl))
				{
					var apiName = apiUrl.Host;
					initContainers.TryAdd(apiName, GetInitContainerToWaitForApi(apiName, kestrelPort));
				}
			}

			foreach (var kvp in context.DeploymentSecrets)
			{
				var name = kvp.Key;
				var value = kvp.Value;
				envVariables.Add(name.VariableName, GetSecretEnvVar(name.VariableName, $"{context.HostInfo.AppName}-secrets", name.StringDataKey));

				if (value == azuriteUrl)
				{
					var match = Regex.Match(value, @"http:\/\/(?<HOST>.+?):10000", RegexOptions.ExplicitCapture);
					if (match.Success)
					{
						string host = match.Groups["HOST"].Value;
						initContainers.TryAdd("azurite", GetInitContainerToWaitForAzurite(host));
					}

				}
				else if (value == sbUrl)
				{
					var match = Regex.Match(value, @"sb:\/\/(?<HOST>.+?);", RegexOptions.ExplicitCapture);
					if (match.Success)
					{
						string host = match.Groups["HOST"].Value;
						initContainers.TryAdd("servicebus", GetInitContainerToWaitForServiceBus(host));
					}
				}
				else if (value.Contains("Data Source="))
				{
					var match = Regex.Match(value, @"Data Source=(?<HOST>.+?);", RegexOptions.ExplicitCapture);
					if (match.Success)
					{
						string host = match.Groups["HOST"].Value;
						initContainers.TryAdd("sqlserver", GetInitContainerToWaitForSqlServer(host));
					}
				}
			}

			await File.WriteAllTextAsync(
				statefulSetPath,
				await _template.RenderStatefulSetTemplate(
					context.HostInfo.AppName,
					options.OtelEnv,
					string.Join(Environment.NewLine, initContainers.Values),
					string.Join(Environment.NewLine, envVariables.Values),
					cancellationToken),
				Encoding.UTF8,
				cancellationToken);

			Console.WriteLine($"Generated {statefulSetPath}");
			return statefulSetPath;
		}

		static string GetInitContainerToWaitForAzurite(string host)
		{
			return @$"      - name: init-wait-azurite
        image: alpine:3
        imagePullPolicy: IfNotPresent
        command: [""sh"", ""-c"", ""for i in $(seq 1 300); do nc -zvw1 {host} 10000 && exit 0 || sleep 3; done; exit 1""]";
		}

		static string GetInitContainerToWaitForServiceBus(string host)
		{
			return @$"      - name: init-wait-servicebus
        image: alpine:3
        imagePullPolicy: IfNotPresent
        command: [""sh"", ""-c"", ""for i in $(seq 1 300); do nc -zvw1 {host} 5672 && exit 0 || sleep 3; done; exit 1""]";
		}

		static string GetInitContainerToWaitForSqlServer(string host)
		{
			return @$"      - name: init-wait-sqlserver
        image: alpine:3
        imagePullPolicy: IfNotPresent
        command: [""sh"", ""-c"", ""for i in $(seq 1 300); do nc -zvw1 {host} 1433 && exit 0 || sleep 3; done; exit 1""]";
		}

		static string GetInitContainerToWaitForApi(string serviceName, int port)
		{
			return @$"      - name: init-wait-{serviceName}
        image: alpine:3
        imagePullPolicy: IfNotPresent
        command: [""sh"", ""-c"", ""for i in $(seq 1 300); do nc -zvw1 {serviceName} {port} && exit 0 || sleep 3; done; exit 1""]";
		}

		static string GetSecretEnvVar(string variableName, string secretKeyRefName, string secretKey)
		{
			return @$"        - name: {variableName}
          valueFrom: 
            secretKeyRef:
              name: {secretKeyRefName}
              key: ""{secretKey}""";
		}
	}
}



