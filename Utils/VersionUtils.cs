using System.Text.RegularExpressions;

using static PathConstants;
public static class VersionUtils
{
    public static string FindCurrentMaxVersionPath()
	{
        string versionRegexPattern = @"^\d\.\d\."; 
        Regex regex = new Regex(versionRegexPattern);


		string maxVersion = "8.0.0.0";
		string maxVersionPath = "";

		foreach (string distributionPath in possible1CDistributionsPaths)
		{			
			if (!Directory.Exists(distributionPath)) continue;
			
			foreach (string dir in Directory.GetDirectories(distributionPath, "*", SearchOption.TopDirectoryOnly))
			{
				string version = dir.Split(@"\")[3];

				if (regex.IsMatch(version))
				{
					if (VersionIsHigher(version, maxVersion))
					{
						maxVersion = version;
						maxVersionPath = dir;
					}
				}
			}
		}
		if (maxVersion == "8.0.0.0") return "";
		return maxVersionPath;
	}

	public static string GetPlatformVersionFromPath(string path)
	{
		return path.Split(@"\")[3];
	}

    public static bool VersionIsHigher(string version1, string version2)
	{
		if (Version.TryParse(version1, out Version v1) && 
			Version.TryParse(version2, out Version v2))
		{
			int comparison = v1.CompareTo(v2);
			
			if (comparison < 0)
				return false;
			
			return true;
		}
		return false;
	}

	// некрасивое решение, но рабочее
	public static string GetConfUpdatePath(string tmpltsPath, string zipFileName)
	{
		string[] tokens = zipFileName.Split('_');

		int tokensCount = tokens.Length;

		string[] result = new string[2];

		// config name
		result[0] = tokens[0];
		// config version
		for (int i = 1; i < tokensCount - 1; i++)
		{
			if (i != tokensCount - 2) result[1] += $"{tokens[i]}_";
			else result[1] += $"{tokens[i]}";
		}

		string onecTemplatesPath = Path.Combine(tmpltsPath, "1c");

		CommonUtils.Log.WriteLine(onecTemplatesPath);

		if (!Directory.Exists(onecTemplatesPath)) return "";

		string confPath = Path.Combine(onecTemplatesPath, result[0]);

		CommonUtils.Log.WriteLine(confPath);
		if (!Directory.Exists(confPath)) return "";

		string confVersionPath = Path.Combine(confPath, result[1]);

		CommonUtils.Log.WriteLine(confVersionPath);
		if (!Directory.Exists(confVersionPath)) return "";

		return confVersionPath;
	}
}