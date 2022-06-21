using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace SearchIcons
{
    internal class ReferenceFinder
    {
        const string TargetProjectName = "Images";
        const string TargetEnumName = "ImagesType";
        const string TargetEnumFileName = "ImagesType.g.cs";
        public async Task<List<string>> FindUsingIconsAsync()
        {
            var icons = new List<string>();
            var temp = new List<string>();
            var helper = new MethodsHelper();

            #region Test
            //
            //var sourceFiles = helper.GetCsFiles().Find(item => item.Contains("AddMailModeEnum."));
            //if (sourceFiles == null)
            //    return null;
            //var sourceText = SourceText.From(new FileStream(sourceFiles, FileMode.Open));
            ////var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
            ////var syntaxNodes = syntaxTree.GetRoot().DescendantNodes();

            //var syntaxNodes = CSharpSyntaxTree
            //    .ParseText(sourceText)
            //    .GetRoot()
            //    .DescendantNodes();

            //var classPropertyes = syntaxNodes.Where(item => item.IsKind(SyntaxKind.IdentifierName));

            //foreach (var property in classPropertyes)
            //{
            //    if (property is IdentifierNameSyntax)
            //    {
            //        var propertyKind = property as IdentifierNameSyntax;
            //        var enumTypeName = propertyKind.Identifier.ValueText;

            //        if (enumTypeName == TargetEnumName)
            //        {
            //            if (propertyKind.Parent is MemberAccessExpressionSyntax)
            //            {
            //                var enumKind = propertyKind.Parent as MemberAccessExpressionSyntax;

            //                var valueText = enumKind.Name.Identifier.ValueText;

            //                //temp.Add(enumTypeName + "." + enumKind.Name.Identifier.ValueText);
            //                temp.Add(valueText);

            //                if (enumKind.Parent is AttributeArgumentSyntax)
            //                {
            //                    //Вычисляем/добавляем иконки, с помощью которых идет адаптация под нужный размер
            //                    //например:для Mail_Collections_24 - это Mail_Collections_16 и Domain_Add_32
            //                    var newList = helper.ConfigureName(valueText);
            //                    foreach (var item in newList)
            //                    {
            //                        temp.Add(item);
            //                    }

            //                }
            //            }
            //        }
            //    }

            //}

            #endregion

            #region Works part
            var sourceFiles = await FindIconsReferenceLocationAsync();

            if (sourceFiles == null)
            {
                return null;
            }

            foreach (var file in sourceFiles)
            {
                var sourceText = SourceText.From(new FileStream(file, FileMode.Open));

                var syntaxNodes = CSharpSyntaxTree
                    .ParseText(sourceText)
                    .GetRoot()
                    .DescendantNodes();

                var classPropertyes = syntaxNodes.Where(item => item.IsKind(SyntaxKind.IdentifierName));

                foreach (var property in classPropertyes)
                {
                    if (property is IdentifierNameSyntax)
                    {
                        var propertyKind = property as IdentifierNameSyntax;
                        var enumTypeName = propertyKind.Identifier.ValueText;

                        if (enumTypeName == TargetEnumName)
                        {
                            if (propertyKind.Parent is MemberAccessExpressionSyntax)
                            {
                                var enumKind = propertyKind.Parent as MemberAccessExpressionSyntax;

                                var valueText = enumKind.Name.Identifier.ValueText;
                                //temp.Add(enumTypeName + "." + enumKind.Name.Identifier.ValueText);

                                temp.Add(valueText);

                                if (enumKind.Parent is AttributeArgumentSyntax)
                                {
                                    //Вычисляем/добавляем иконки, с помощью которых идет адаптация под нужный размер
                                    //например:для Mail_Collections_24 - это Mail_Collections_16 и Domain_Add_32
                                    var newList = helper.ConfigureName(valueText);
                                    foreach (var item in newList)
                                    {
                                        temp.Add(item);
                                    }

                                }
                            }
                        }
                    }
                }
                #endregion
                //icons = temp.Distinct().ToList();
                //return icons;
            }

            icons = temp.Distinct().ToList();            
            return icons;
        }


        //returns a list of paths to cs-files with references
        public async Task<List<string>> FindIconsReferenceLocationAsync()
        {
            var iconReferencesList = new List<ReferenceLocation>();
            var pathFilesList = new List<string>();
            var resultList = new List<string>();
            var methods = new MethodsHelper();

            var solution = await SolutionCheckerInstance.Instance;
            var projects = solution.Projects;         
            
            foreach (var project in projects)
            {
                var compilation = project.GetCompilationAsync().Result;
                if (project.Name != TargetProjectName)
                {
                    continue;
                }
                
                foreach (var document in project.Documents)
                {
                    if (!document.SupportsSyntaxTree || document.Name != TargetEnumFileName)
                    {
                        continue;
                    } 
                    
                    try
                    {
                        var semanticModel = await document.GetSemanticModelAsync();

                        var syntaxNode = await document.GetSyntaxRootAsync();

                        var nodes = syntaxNode?.DescendantNodes();

                        var enumDeclarationSyntax = nodes?.OfType<EnumDeclarationSyntax>();

                        foreach (var declarationSyntax in enumDeclarationSyntax)
                        {
                            if (declarationSyntax == null) continue;
                            if (declarationSyntax.Identifier.Text == TargetEnumName)
                            {
                                var enumSymbol = semanticModel.GetDeclaredSymbol(declarationSyntax) as ITypeSymbol;

                                var referencesToEnum = await SymbolFinder.FindReferencesAsync(enumSymbol, document.Project.Solution);

                                foreach (var reference in referencesToEnum)
                                {
                                    foreach (var location in reference.Locations)
                                    {
                                        iconReferencesList.Add(location);
                                        pathFilesList.Add(location.Document.FilePath);
                                    }
                                }
                            }
                        }

                    }
                    catch
                    {
                        continue;
                    }

                }
            }

            //methods.DeleteTargetFile();
            resultList = pathFilesList.Distinct().ToList();
            return resultList;
        }
        //не работает
        public void AddTargetFile()
        {
            var partFile = @"obj\Debug\GeneratedImagesPalettes";
            var projectPart = @"[Common]\.NET\Images";
            var solutionName = @"SecureTower.NET.sln";
            var baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var basePart = baseDirectory.Split(System.AppDomain.CurrentDomain.FriendlyName)[0];
            var filePath = Path.Combine(basePart, projectPart, partFile, TargetEnumFileName);
            var projectPath = Path.Combine(basePart, projectPart);
            var dotNetSolutionFile = Path.Combine(basePart, solutionName);

            var solution = SolutionCheckerInstance.Instance;
            var projects = solution.Result.Projects;

            foreach (var project in projects)
            {
                if (project.Name != TargetProjectName)
                {
                    continue;
                }
                var fileText = new StringBuilder();
                var textFileStream = File.OpenText(filePath);
                fileText.Append(textFileStream.ReadToEnd());
                textFileStream.Close();

                var sourceText = SourceText.From(fileText.ToString());

                //targetProject.AddAdditionalDocument();
                var newDocument = project.AddDocument(TargetEnumFileName, sourceText, null, projectPath).GetType();
                var workSpace = MSBuildWorkspace.Create();
                var solution1 = workSpace.OpenSolutionAsync(dotNetSolutionFile);

                //var doc = project.AddDocument("index.html", "<html></html>");
                //workspace.ApplyChanges(originalSolution, doc.Project.Solution);
            }
            //    var targetProject = projects.Select(x => x.Name == TargetProjectName) as Project;

            //if (targetProject == null)
            //{
            //    return;
            //}           

        }
    }
}
