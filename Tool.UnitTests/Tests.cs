using Microsoft.Build.Locator;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Tool.UnitTests
{
    [SetUpFixture]
    internal class MSBuildLocatorSetUp
    {
        [OneTimeSetUp] public void Register()
        {
            MSBuildLocator.RegisterDefaults();
        }
    }

    [TestFixture]
    public class LoaderTests
    {
        [Test] public void TestLoad()
        {
            var content = @"<Project Sdk=""Microsoft.NET.Sdk"" DefaultTargets=""Build"">
  <PropertyGroup>
    <AssemblyName>$(MSBuildProjectName)</AssemblyName>
    <RootNamespace>$(AssemblyName)</RootNamespace>
    <TargetFramework>net471</TargetFramework>
  </PropertyGroup>
</Project>";
            using (var temp = new TempDirectory())
            {
                var path = Path.Combine(temp.Directory.Path, "Project.csproj");
                File.WriteAllText(path, content);
                var properties = Loader.LoadProperties(path);
                var usingSDKProperty = properties.SingleOrDefault(p => p.Name.Equals("UsingMicrosoftNETSdk", StringComparison.OrdinalIgnoreCase));
                var usingSDK = bool.Parse(usingSDKProperty?.EvaluatedValue ?? "false");
                Console.WriteLine($"Using .NET Core SDK: {usingSDK}");
            }
        }
    }

    public sealed class TempDirectory : IDisposable
    {
        public TempDirectory([CallerMemberName] string caller = "")
        {
            Directory = DirectoryPath.Temp.CreateUnique(caller);
        }
        public DirectoryPath Directory { get; }
        public void Dispose()
        {
            Directory.DeleteIgnoringErrors();
        }
    }
    public sealed class DirectoryPath
    {
        public static DirectoryPath Temp => new DirectoryPath(System.IO.Path.GetTempPath());
        public DirectoryPath(string path)
        {
            Path = System.IO.Path.GetFullPath(path).TrimEnd(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
        }
        public string Path { get; }
        public bool Exists => Directory.Exists(Path);
        private void Create()
        {
            Directory.CreateDirectory(Path);
        }
        private void CreateNew()
        {
            DirectoryPath temp;
            do
            {
                temp = new DirectoryPath(Path.TrimEnd(System.IO.Path.DirectorySeparatorChar) + "." + Guid.NewGuid());
            }
            while (temp.Exists);
            try
            {
                temp.Create();
                Directory.Move(temp.Path, Path);
            }
            finally
            {
                if (temp.Exists)
                    temp.DeleteIgnoringErrors();
            }
        }
        public DirectoryPath CreateUnique(string name)
        {
            var index = 0;
            while (true)
            {
                var temp = new DirectoryPath(System.IO.Path.Combine(Path, name + (index == 0 ? "" : "-" + index)));
                try
                {
                    temp.CreateNew();
                    return temp;
                }
                catch (Exception e)
                {
                    if (index >= 1000)
                        throw new IOException($"Failed to create {System.IO.Path.Combine(Path, name)}", e);
                    index++;
                }
            }
        }
        public void DeleteIgnoringErrors()
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
    }
}