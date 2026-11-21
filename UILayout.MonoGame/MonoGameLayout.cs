using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UILayout
{
    public partial class MonoGameLayout : Layout
    {        
        public static new MonoGameLayout Current { get { return Layout.Current as MonoGameLayout; } }
        public static string DefaultTextureNamespace { get; set; } = "UILayout";

        public MonoGameHost Host { get; private set; }

        /// <summary>
        /// The app's own scale (for example a user setting). Drawing and input use Scale x DisplayScale.
        /// </summary>
        public float Scale
        {
            get => scale;
            set
            {
                scale = value;

                ApplyScale();
            }
        }

        /// <summary>
        /// The display's scale (its DPI; 1 = 96 DPI), set by the backend: from the window's DPI for standalone apps,
        /// from the plugin host for plugin editors. Apps normally leave it alone and use Scale for their own zoom.
        /// </summary>
        public float DisplayScale
        {
            get => displayScale;
            set
            {
                displayScale = (value > 0) ? value : 1.0f;

                ApplyScale();
            }
        }

        /// <summary>
        /// The scale drawing and input use: Scale x DisplayScale (window pixels per layout unit).
        /// </summary>
        public float TotalScale => scale * displayScale;

        public RectF UnscaledBounds { get; private set; } = RectF.Empty;

        float scale = 1.0f;
        float displayScale = 1.0f;

        void ApplyScale()
        {
            float total = TotalScale;

            if (GraphicsContext != null)
                GraphicsContext.Scale = total;

            if (!UnscaledBounds.IsEmpty)
            {
                base.SetBounds(new RectF(UnscaledBounds.X, UnscaledBounds.Y, UnscaledBounds.Width / total, UnscaledBounds.Height / total));
            }
        }

        public override bool InputIsActive
        {
            get
            {
                return Host.IsActive;
            }
        }

        public MonoGameLayout()
        {
        }

        public virtual void SetHost(MonoGameHost host)
        {
            this.Host = host;

            GraphicsContext = new GraphicsContext2D(new SpriteBatch(Host.GraphicsDevice)) { Scale = TotalScale };

            UIImage singleWhitePixelImage = new UIImage(1, 1);

            UIColor[] data = new UIColor[1];
            data[0] = UIColor.White;

            singleWhitePixelImage.SetData(data);

            GraphicsContext.SingleWhitePixelImage = singleWhitePixelImage;

            var loader = new AssemblyResourceContentLoader(typeof(UILayout.DefaultTextures.TextureLoader).Assembly, DefaultTextureNamespace);

            UILayout.DefaultTextures.TextureLoader.LoadDefaultTextures(loader);
        }

        public void LoadImageManifest(ContentLoader loader, string manifestName)
        {
            using (Stream manifestStream = loader.OpenContentStream(Path.Combine("Textures", manifestName)))
            {
                ImageManifest.Load(loader, manifestStream, this);
            }
        }

        public override Task<string> GetKeyboardInputAsync(string title, string defaultText)
        {
#if (WINDOWS && !MONOGL) || ANDROID
            return KeyboardInput.Show(title, null, defaultText);
#else
            try
            {
                Process process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "zenity";
                process.StartInfo.Arguments = "--entry --title=\"" + title + "\" --entry-text=\"" + defaultText + "\"";

                process.Start();
                return process.StandardOutput.ReadToEndAsync();
            }
            catch { }
#endif

            return null;
        }

        public override void SetBounds(in RectF bounds)
        {
            this.UnscaledBounds = bounds;

            base.SetBounds(new RectF(bounds.X, bounds.Y, bounds.Width / TotalScale, bounds.Height / TotalScale));
        }

        public override void Draw(UIElement startElement)
        {
            GraphicsContext.BeginDraw();

            base.Draw(startElement);

            GraphicsContext.EndDraw();
        }
    }
}
