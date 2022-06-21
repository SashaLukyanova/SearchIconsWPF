using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace SearchIcons
{
    internal static class SolutionCheckerInstance
    {        
        private const string SolutionName = @"SecureTower.NET.sln";
        private static readonly Lazy<Task<Solution>> Lazy = new Lazy<Task<Solution>>(LoadProjectAsync);

        public static Task<Solution> Instance => Lazy.Value;

        private static async Task<Solution> LoadProjectAsync()
        {
            var methods = new MethodsHelper();
            methods.CopyTargetFile();

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var basePath = baseDirectory.Split(AppDomain.CurrentDomain.FriendlyName)[0];
            var dotNetSolutionFile = Path.Combine(basePath, SolutionName);

            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToList();
            var registeredInstance = visualStudioInstances.FirstOrDefault(vsInstance => vsInstance.Version.Major == 16) ??
                                     visualStudioInstances.First();
            MSBuildLocator.RegisterInstance(registeredInstance);
            var workSpace = MSBuildWorkspace.Create();
            var solution = await workSpace.OpenSolutionAsync(dotNetSolutionFile).ConfigureAwait(false);
            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents.Where(item => item.SupportsSyntaxTree))
                {
                    await document.GetSyntaxTreeAsync().ConfigureAwait(false);
                }
            }

            return solution;
        }
    }
}
