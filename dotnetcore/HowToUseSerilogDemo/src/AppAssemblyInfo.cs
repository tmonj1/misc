using System.Reflection;

namespace SerilogDemo
{
    /// <summary>
    /// Provides properties of the assembly.
    /// </summary>
    public static class AppAssemblyInfo
    {
        /// <summary>
        /// Application Name
        /// </summary>
        private static string _appName = null;

        /// <summary>
        /// Version
        /// </summary>
        private static string _appVersion = null;

        static AppAssemblyInfo()
        {
            CacheAssemblyInfoOnce();
        }

        /// <summary>
        /// Gets Application Name.
        /// </summary>
        /// <value></value>
        public static string AppName
        {
            get
            {
                return _appName;
            }
        }

        /// <summary>
        /// Gets semver (or assembly version if semver dosen't exist)
        /// </summary>
        /// <value></value>
        public static string AppVersion
        {
            get
            {
                return _appVersion;
            }
        }
        /// <summary>
        /// Caches assembly info if it does not exists.
        /// </summary>
        private static void CacheAssemblyInfoOnce()
        {
            if (string.IsNullOrEmpty(_appName))
            {
                var assembly = Assembly.GetEntryAssembly();

                // app name
                var nameAndAttributes = assembly.FullName.Split(",");
                _appName = nameAndAttributes[0];

                // version
                var infoVer = assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))
                    as AssemblyInformationalVersionAttribute;
                if (infoVer != null)
                {
                    _appVersion = infoVer.InformationalVersion;
                }
                else if (nameAndAttributes.Length > 1)
                {
                    _appVersion = nameAndAttributes[1].Replace("Version= ", "");
                }
            }
        }
    }
}