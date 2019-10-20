namespace MigrationHelper.DataObjects
{
    public class ReferenceEntry
    {
        /// <summary>
        /// Gets or sets the name of the package
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the package is a development dependency
        /// </summary>
        public bool IsDevelopmentDependency { get; set; }

        /// <summary>
        /// Gets or sets the target framework
        /// </summary>
        public string TargetFramework { get; set; }

        /// <summary>
        /// Gets the full name of the .NET target framework
        /// </summary>
        public string TargetFrameworkName
        {
            get
            {
                switch (TargetFramework)
                {
                    case "net11":
                        return ".NET Framework 1.1";
                    case "net20":
                        return ".NET Framework 2";
                    case "net35":
                        return ".NET Framework 3.5";
                    case "net40":
                        return ".NET Framework 4";
                    case "net403":
                        return ".NET Framework 4.0.3";
                    case "net45":
                        return ".NET Framework 4.5";
                    case "net451":
                        return ".NET Framework 4.5.1";
                    case "net452":
                        return ".NET Framework 4.5.2";
                    case "net46":
                        return ".NET Framework 4.6";
                    case "net461":
                        return ".NET Framework 4.6.1";
                    case "net462":
                        return ".NET Framework 4.6.2";
                    case "net47":
                        return ".NET Framework 4.7";
                    case "net471":
                        return ".NET Framework 4.7.1";
                    case "net472":
                        return ".NET Framework 4.7.2";
                    case "net48":
                        return ".NET Framework 4.8";
                    default:
                        return "undefined";
                }
            }
        }
    }
}
