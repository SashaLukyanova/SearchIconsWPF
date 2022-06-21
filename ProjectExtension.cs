using Microsoft.CodeAnalysis;

namespace SearchIcons
{
    public static class ProjectExtension
    {
        const string TargetProjectName = "Images";
        const string TargetEnumName = "ImagesType";
        const string TargetEnumFileName = "ImagesType.g.cs";
        public static void DocumentsAdd(this Project proj, string path)
        {
        }
    }
}
