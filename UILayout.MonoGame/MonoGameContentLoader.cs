using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace UILayout
{
    public class MonoGameContentLoader : ContentLoader
    {
        ContentManager contentManager;

        public MonoGameContentLoader(ContentManager contentManager)
        {
            this.contentManager = contentManager;
        }

        public override Stream OpenContentStream(string contentPath)
        {
            // Open raw content the way the content manager opens assets: next to the assembly for an
            // AssemblyRelativeContentManager (the Windows Game backend, and the embedded backend - inside a plugin
            // host, TitleContainer's location is the host application's), otherwise through TitleContainer
            if (contentManager is AssemblyRelativeContentManager)
                return AssemblyRelativeContentManager.OpenAseemblyRelativeStream(Path.Combine(contentManager.RootDirectory, contentPath));

            return TitleContainer.OpenStream(Path.Combine(contentManager.RootDirectory, contentPath));
        }

        public override UIImage LoadImage(string imageName)
        {
            return new UIImage(contentManager.Load<Texture2D>(Path.Combine("Textures", imageName)));
        }
    }
}
