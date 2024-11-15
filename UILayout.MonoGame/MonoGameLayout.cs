using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
#if WINDOWS
using System.Windows.Forms;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UILayout
{
    public class MonoGameLayout : Layout
    {        
        public static new MonoGameLayout Current { get { return Layout.Current as MonoGameLayout; } }

        public MonoGameHost Host { get; private set; }
        public float Scale
        {
            get => scale;
            set
            {
                scale = value;

                if (GraphicsContext != null)
                    GraphicsContext.Scale = scale;

                if (!UnscaledBounds.IsEmpty)
                {
                    base.SetBounds(new RectF(UnscaledBounds.X, UnscaledBounds.Y, UnscaledBounds.Width / scale, UnscaledBounds.Height / scale));
                }
            }
        }
        public RectF UnscaledBounds { get; private set; } = RectF.Empty;

        float scale = 1.0f;

        public override bool InputIsActive
        {
            get
            {
#if WINDOWS
                return System.Windows.Forms.Form.ActiveForm == (System.Windows.Forms.Control.FromHandle(Host.Window.Handle) as System.Windows.Forms.Form);
#else
                return true;
#endif
            }
        }

        public MonoGameLayout()
        {
        }

        public virtual void SetHost(Game host)
        {
            this.Host = host as MonoGameHost;

            GraphicsContext = new GraphicsContext2D(new SpriteBatch(Host.GraphicsDevice)) { Scale = Scale };
        }

        public void LoadImageManifest(string manifestName)
        {
            using (Stream manifestStream = Host.OpenContentStream(Path.Combine("Textures", manifestName)))
            {
                ImageManifest.Load(manifestStream, this);
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

        public override string GetFolder(string title, string initialPath)
        {
#if WINDOWS
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = title;
            dialog.SelectedPath = initialPath;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
#else
            try
            {
                Process process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "zenity";
                process.StartInfo.Arguments = "--file-selection --filename=" + initialPath + " --directory";

                process.Start();
                return process.StandardOutput.ReadToEnd().Trim();
            }
            catch { }
#endif

            return null;
        }

        public override string GetFile(string title, string initialPath, string filePattern)
        {
#if WINDOWS
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = title;
            dialog.Filter = filePattern;
            dialog.InitialDirectory = initialPath;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.FileName;
            }
#else
            try
            {
                Process process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "zenity";
                process.StartInfo.Arguments = "--file-selection --filename=" + initialPath;

                process.Start();
                return process.StandardOutput.ReadToEnd().Trim();
            }
            catch { }
#endif

            return null;
        }

        public override void SetBounds(in RectF bounds)
        {
            this.UnscaledBounds = bounds;

            base.SetBounds(new RectF(bounds.X, bounds.Y, bounds.Width / Scale, bounds.Height / Scale));
        }

        public override void Draw(UIElement startElement)
        {
            GraphicsContext.BeginDraw();

            base.Draw(startElement);

            GraphicsContext.EndDraw();
        }
    }
}
