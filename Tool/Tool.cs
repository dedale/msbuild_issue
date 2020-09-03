using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using System;
using System.Collections.Immutable;

namespace Tool
{
    public static class Loader
    {
        public static ImmutableArray<ProjectProperty> LoadProperties(string path)
        {
            using (var coll = new ProjectCollection())
            {
                var p = coll.LoadProject(path);
                return p.Properties.ToImmutableArray();
            }
        }
    }
    internal static class Program
    {
        internal static void Main()
        {
            MSBuildLocator.RegisterDefaults();
            Console.WriteLine("More stuff here...");
        }
    }
}
