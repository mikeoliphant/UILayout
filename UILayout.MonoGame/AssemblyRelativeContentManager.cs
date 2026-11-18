using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace UILayout
{
    public class AssemblyRelativeContentManager : ContentManager
    {
        public static String AssemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static Stream OpenAseemblyRelativeStream(string relativePath)
        {
            return File.OpenRead(Path.Combine(AssemblyPath, relativePath));
        }

        public AssemblyRelativeContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected override Stream OpenStream(string assetName)
        {
            return OpenAseemblyRelativeStream(Path.Combine(RootDirectory, assetName) + ".xnb");
        }
    }
}
