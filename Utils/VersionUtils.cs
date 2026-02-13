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
}