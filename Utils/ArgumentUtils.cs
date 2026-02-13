public static class ArgumentUtils
{
    public static string GetAuthString(string login, string password)
    {
        if (!string.IsNullOrEmpty(login))
        {
            if (!string.IsNullOrEmpty(password)) 
                return string.Format(@"/N ""{0}"" /P ""{1}""", login, password);

            return string.Format(@"/N ""{0}""", login);
        }
        
        return "";
    }

    public enum ONEC { ENTERPRISE, DESIGNER }
    public static string FormResultString(ONEC mode, string IBConnectionString, string authString)
    {
        return $"{mode} /IBConnectionString {IBConnectionString} {authString}" ;
    }
}