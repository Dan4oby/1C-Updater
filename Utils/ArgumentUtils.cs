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

    private static string FormArgumentString(string mode, string IBConnectionString, string baseLogin, string basePassword)
    {
        string authString = GetAuthString(baseLogin, basePassword);
        return $"{mode} /IBConnectionString {IBConnectionString} {authString} /DisableStartupDialogs /DisableStartupMessages" ;
    }

    public static string FormArgumentStringDesigner(string IBConnectionString, string baseLogin, string basePassword)
    {
        return FormArgumentString("DESIGNER", IBConnectionString, baseLogin, basePassword);
    }

    public static string FormArgumentStringEnterprise(string IBConnectionString, string baseLogin, string basePassword)
    {
        return FormArgumentString("ENTERPRISE", IBConnectionString, baseLogin, basePassword);
    }
}