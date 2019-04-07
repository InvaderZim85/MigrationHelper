namespace MigrationHelper.DataObjects
{
    public class ReferenceEntry
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsDevelopmentDependency { get; set; }
        public string TargetFramework { get; set; }
    }
}
