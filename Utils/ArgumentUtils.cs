public static class ArgumentUtils
{
    static string GetAuthString(string login, string password)
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
    public static string FormArgumentString(ONEC mode, string IBConnectionString, string baseLogin, string basePassword)
    {
        string authString = GetAuthString(baseLogin, basePassword);
        return $"{mode} /IBConnectionString {IBConnectionString} {authString}" ;
    }
}