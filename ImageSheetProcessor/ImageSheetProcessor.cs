using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Xml.Serialization;
using UILayout;

namespace ImageSheetProcessor
{
    public class ImageSheetEntry
    {
        public Bitmap Bitmap { get; set; }
        public string NamePrefix { get; set; }
    }

    public class ImageSheetProcessor
    {
        public string SrcPath { get; set; }
        public string DestPath { get; set; }
        public int VerticalPadding { get; set; }
        public bool ForceRegen { get; set; }
        public bool PadToPowerOfTwo { get; set; }
        public int MaxImageSheetSize { get; set; }
        public int ImageGutterSize { get; set; }
        public bool WriteToDDS { get; set; }
        public UIColor ImageMaskColor { get; set; }
        public string ImageNamePrefix { get; set; }
        public bool InSpriteSheet
        {
            get { return imageSheetGroupName != null; }
        }

        protected Bitmap overlayTexture;
        protected ImageManifest imageManifest;
        protected Dictionary<string, ImageManifestSheetImage> imageDictionary = new Dictionary<string, ImageManifestSheetImage>();
        protected XmlSerializer serializer;
        protected string imageSheetGroupName;
        protected ImageManifestSheet imageSheet;
        protected Dictionary<string, ImageSheetEntry> imageSheetGroupImages = new Dictionary<string, ImageSheetEntry>();
        protected string tmpDir;
        protected List<string> directoryStack = new List<string>();

        Dictionary<int, int> jisMapping;

        public ImageSheetProcessor()
        {
            PadToPowerOfTwo = true;
            MaxImageSheetSize = 1024;
            ImageGutterSize = 1;
            ImageMaskColor = UIColor.Transparent;
            serializer = new XmlSerializer(typeof(ImageManifest));
        }

        public void SetOverlayTexture(string textureName)
        {
            if (string.IsNullOrEmpty(textureName))
            {
                overlayTexture = null;
            }
            else
            {
                overlayTexture = (Bitmap)Bitmap.FromFile(Path.Combine(SrcPath, textureName));
            }
        }

        public void BeginRenderImages(string destPath)
        {
            DestPath = destPath;

            tmpDir = Path.Combine(SrcPath, "tmp");

            if (!Directory.Exists(tmpDir))
            {
                Directory.CreateDirectory(tmpDir);
            }

            imageManifest = null;

            if (!ForceRegen)
            {
                try
                {
                    using (Stream reader = File.Open(Path.Combine(DestPath, "ImageManifest.xml"), FileMode.Open))
                    {
                        imageManifest = serializer.Deserialize(reader) as ImageManifest;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            if (imageManifest == null)
                imageManifest = new ImageManifest();

            imageDictionary.Clear();

            imageManifest.SpriteSheets.Clear();
        }

        public void EndRenderImages()
        {
            using (Stream writer = File.Open(Path.Combine(DestPath, "ImageManifest.xml"), FileMode.Create))
            {
                serializer.Serialize(writer, imageManifest);
            }
        }

        public void PushDirectory(string directory)
        {
            directoryStack.Insert(directoryStack.Count, directory);
        }

        public void PopDirectory()
        {
            directoryStack.RemoveAt(directoryStack.Count - 1);
        }

        public string GetSourcePath(string imageName)
        {
            return GetSourcePath(imageName, ".png");
        }

        public string GetSourcePath(string imageName, string extension)
        {

            return Path.Combine(GetSourcePath(), imageName) + extension;
        }

        public string GetSourcePath()
        {
            string path = SrcPath;

            foreach (string directory in directoryStack)
            {
                path = Path.Combine(path, directory);
            }

            return path;
        }

        public void BeginSpriteSheetGroup(string groupName)
        {
            BeginSpriteSheetGroup(groupName, false, false);
        }

        public void BeginSpriteSheetGroup(string groupName, bool requireReadbleImages, bool generateImageMasks)
        {
            this.imageSheetGroupName = groupName;

            imageSheetGroupImages.Clear();

            imageSheet = new ImageManifestSheet();
        }

        public void EndSpriteSheetGroup()
        {
            int sheetNum = 0;

            MaxRectsBinPack binPack = new MaxRectsBinPack(MaxImageSheetSize, MaxImageSheetSize, false);

            List<string> images = new List<string>(imageSheetGroupImages.Keys);

            Bitmap outBitmap = new Bitmap(MaxImageSheetSize, MaxImageSheetSize);
            Graphics renderer = Graphics.FromImage(outBitmap);
            renderer.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            images.Sort(delegate (string s1, string s2) { return Math.Max(imageSheetGroupImages[s2].Bitmap.Height, imageSheetGroupImages[s2].Bitmap.Width).
                CompareTo(Math.Max(imageSheetGroupImages[s1].Bitmap.Height, imageSheetGroupImages[s1].Bitmap.Width)); });

            do
            {
                string filename;
                ImageSheetEntry imageBitmap;
                Rectangle outRect;

                int pos = 0;

                do
                {
                    filename = images[pos];

                    imageBitmap = imageSheetGroupImages[filename];

                    outRect = binPack.Insert(imageBitmap.Bitmap.Width + (ImageGutterSize * 2), imageBitmap.Bitmap.Height + (ImageGutterSize * 2), MaxRectsBinPack.FreeRectChoiceHeuristic.RectContactPointRule);

                    if (outRect.Height > 0)
                        break;

                    pos++;
                }
                while (pos < images.Count);

                if (outRect.Height > 0)
                {
                    DrawImageWithGutter(imageBitmap.Bitmap, renderer, outRect);

                    outRect.X += ImageGutterSize;
                    outRect.Y += ImageGutterSize;
                    outRect.Width -= (ImageGutterSize * 2);
                    outRect.Height -= (ImageGutterSize * 2);

                    ImageManifestSheetImage imageEntry = new ImageManifestSheetImage();
                    imageEntry.ImageName = Path.GetFileNameWithoutExtension(filename);

                    if (!String.IsNullOrEmpty(imageBitmap.NamePrefix))
                        imageEntry.ImageName = imageBitmap.NamePrefix + imageEntry.ImageName;

                    imageEntry.XOffset = outRect.X;
                    imageEntry.YOffset = outRect.Y;
                    imageEntry.Width = outRect.Width;
                    imageEntry.Height = outRect.Height;

                    imageSheet.Images.Add(imageEntry);

                    images.RemoveAt(pos);
                }

                if ((outRect.Height == 0) || (images.Count == 0))
                {
                    imageSheet.SheetName = imageSheetGroupName + sheetNum;
                    imageSheet.SheetWidth = MaxImageSheetSize;
                    imageSheet.SheetHeight = MaxImageSheetSize;
                    imageManifest.SpriteSheets.Add(imageSheet);

                    if (images.Count == 0)
                    {
                        Bitmap shrunk = AttemptShrink(imageSheet, outBitmap);

                        if (shrunk != null)
                        {
                            outBitmap.Dispose();

                            outBitmap = shrunk;
                        }
                    }

                    SaveImage(outBitmap, Path.Combine(DestPath, imageSheetGroupName + sheetNum + ".png"));

                    outBitmap.Dispose();

                    sheetNum++;

                    binPack.Init(MaxImageSheetSize, MaxImageSheetSize, false);

                    outBitmap = new Bitmap(MaxImageSheetSize, MaxImageSheetSize);
                    renderer = Graphics.FromImage(outBitmap);
                    imageSheet = new ImageManifestSheet();
                }
            }
            while (images.Count > 0);

            this.imageSheetGroupName = null;
            this.imageSheet = null;
        }

        void DrawImageWithGutter(Bitmap imageBitmap, Graphics renderer, Rectangle outRect)
        {
            Rectangle srcRect;
            Rectangle destRect;

            // Top
            srcRect = new Rectangle(0, 0, imageBitmap.Width, 1);
            destRect = new Rectangle(outRect.X + ImageGutterSize, outRect.Y, outRect.Width - (ImageGutterSize * 2), ImageGutterSize);
            renderer.DrawImage(imageBitmap, destRect, srcRect, GraphicsUnit.Pixel);

            // Bottom
            srcRect = new Rectangle(0, imageBitmap.Height - 1, imageBitmap.Width, 1);
            destRect = new Rectangle(outRect.X + ImageGutterSize, outRect.Bottom - ImageGutterSize, outRect.Width - (ImageGutterSize * 2), ImageGutterSize);
            renderer.DrawImage(imageBitmap, destRect, srcRect, GraphicsUnit.Pixel);

            // Left
            srcRect = new Rectangle(0, 0, 1, imageBitmap.Height);
            destRect = new Rectangle(outRect.X, outRect.Y + ImageGutterSize, ImageGutterSize, outRect.Height - (ImageGutterSize * 2));
            renderer.DrawImage(imageBitmap, destRect, srcRect, GraphicsUnit.Pixel);

            // Right
            srcRect = new Rectangle(imageBitmap.Width - 1, 0, 1, imageBitmap.Height);
            destRect = new Rectangle(outRect.Right - ImageGutterSize, outRect.Y + ImageGutterSize, ImageGutterSize, outRect.Height - (ImageGutterSize * 2));
            renderer.DrawImage(imageBitmap, destRect, srcRect, GraphicsUnit.Pixel);

            // TopLeft
            srcRect = new Rectangle(0, 0, 1, 1);
            destRect = new Rectangle(outRect.X, outRect.Y, ImageGutterSize, ImageGutterSize);
            renderer.DrawImage(imageBitmap, destRect, srcRect, GraphicsUnit.Pixel);

            // TopRight
            srcRect = new Rectangle(imageBitmap.Width - 1, 0, 1, 1);
            destRect = new Rectangle(outRect.Right - ImageGutterSize, outRect.Y, ImageGutterSize, ImageGutterSize);
            renderer.DrawImage(imageBitmap, destRect, srcRect, GraphicsUnit.Pixel);

            // BottomLeft
            srcRect = new Rectangle(0, imageBitmap.Height - 1, 1, 1);
            destRect = new Rectangle(outRect.X, outRect.Bottom - ImageGutterSize, ImageGutterSize, ImageGutterSize);
            renderer.DrawImage(imageBitmap, destRect, srcRect, GraphicsUnit.Pixel);

            // BottomLeft
            srcRect = new Rectangle(imageBitmap.Width - 1, imageBitmap.Height - 1, 1, 1);
            destRect = new Rectangle(outRect.Right - ImageGutterSize, outRect.Bottom - ImageGutterSize, ImageGutterSize, ImageGutterSize);
            renderer.DrawImage(imageBitmap, destRect, srcRect, GraphicsUnit.Pixel);

            outRect.X += ImageGutterSize;
            outRect.Y += ImageGutterSize;
            outRect.Width -= (ImageGutterSize * 2);
            outRect.Height -= (ImageGutterSize * 2);

            renderer.DrawImage(imageBitmap, outRect);
        }

        Bitmap AttemptShrink(ImageManifestSheet imageSheet, Bitmap sourceBitmap)
        {
            int width = imageSheet.SheetWidth;
            int height = imageSheet.SheetHeight;

            List<ImageManifestSheetImage> newImages = new List<ImageManifestSheetImage>();

            if (width > height)
            {
                width /= 2;
            }
            else
            {
                height /= 2;
            }

            MaxRectsBinPack binPack = new MaxRectsBinPack(width, height, false);
            Bitmap outBitmap = new Bitmap(width, height);
            Graphics renderer = Graphics.FromImage(outBitmap);
            renderer.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            newImages.Clear();

            foreach (ImageManifestSheetImage sprite in imageSheet.Images)
            {
                int gutterWidth = ImageGutterSize;
                int gutterHeight = ImageGutterSize;

                if (sprite.Width == width)
                    gutterWidth = 0;

                Rectangle outRect = binPack.Insert(sprite.Width + (gutterWidth * 2), sprite.Height + (gutterHeight * 2), MaxRectsBinPack.FreeRectChoiceHeuristic.RectContactPointRule);

                if (outRect.Height == 0)
                {
                    outBitmap.Dispose();

                    return null;
                }

                renderer.DrawImage(sourceBitmap, outRect, new Rectangle(sprite.XOffset - gutterWidth, sprite.YOffset - gutterHeight, sprite.Width + (gutterWidth * 2), sprite.Height + (gutterHeight * 2)), GraphicsUnit.Pixel);

                outRect.X += gutterWidth;
                outRect.Y += gutterHeight;
                outRect.Width -= (gutterWidth * 2);
                outRect.Height -= (gutterHeight * 2);

                ImageManifestSheetImage spriteEntry = new ImageManifestSheetImage();
                spriteEntry.ImageName = sprite.ImageName;
                spriteEntry.XOffset = outRect.X;
                spriteEntry.YOffset = outRect.Y;
                spriteEntry.Width = outRect.Width;
                spriteEntry.Height = outRect.Height;

                newImages.Add(spriteEntry);
            }

            imageSheet.SheetWidth = width;
            imageSheet.SheetHeight = height;
            imageSheet.Images = newImages;

            Bitmap shrunk = AttemptShrink(imageSheet, outBitmap);

            if (shrunk != null)
            {
                outBitmap.Dispose();

                return shrunk;
            }
            else
            {
                return outBitmap;
            }
        }

        public void Overlay(string texture1, string texture2, string outTexture)
        {
            Bitmap image1 = (Bitmap)Bitmap.FromFile(Path.Combine(DestPath, texture1) + ".png");
            Bitmap image2 = (Bitmap)Bitmap.FromFile(Path.Combine(DestPath, texture2) + ".png");

            Bitmap outImage = new Bitmap(image1);

            Graphics renderer = Graphics.FromImage(outImage);

            int xOffset = (image1.Width - image2.Width) / 2;
            int yOffset = (image1.Height - image2.Height) / 2;

            renderer.DrawImage(image2, xOffset, yOffset);

            image1.Dispose();
            image2.Dispose();

            SaveAndManifest(outImage, Path.Combine(DestPath, outTexture + ".png"));
        }

        public void ToBlack(Bitmap srcBitmap)
        {
            byte a = 0;

            try
            {
                BitmapData srcData = srcBitmap.LockBits(new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* srcPixels = (byte*)srcData.Scan0;

                    for (int x = 0; x < srcBitmap.Width; x++)
                    {
                        for (int y = 0; y < srcBitmap.Height; y++)
                        {
                            byte* srcOffset = srcPixels + (x * 4) + (y * srcData.Stride);

                            a = *(srcOffset + 3);

                            if (a == 255)
                            {
                                *(srcOffset) = 0;
                                *(srcOffset + 1) = 0;
                                *(srcOffset + 2) = 0;
                            }
                        }
                    }
                }

                srcBitmap.UnlockBits(srcData);
            }
            catch { }
        }

        public void ToBlackExceptPurple(Bitmap srcBitmap)
        {
            byte a = 0;

            try
            {
                BitmapData srcData = srcBitmap.LockBits(new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* srcPixels = (byte*)srcData.Scan0;

                    for (int x = 0; x < srcBitmap.Width; x++)
                    {
                        for (int y = 0; y < srcBitmap.Height; y++)
                        {
                            byte* srcOffset = srcPixels + (x * 4) + (y * srcData.Stride);

                            a = *(srcOffset + 3);

                            if (a == 255)
                            {
                                if (*(srcOffset + 1) == 0)
                                {
                                    *(srcOffset + 3) = 0;
                                }

                                *(srcOffset) = 0;
                                *(srcOffset + 1) = 0;
                                *(srcOffset + 2) = 0;
                            }
                        }
                    }
                }

                srcBitmap.UnlockBits(srcData);
            }
            catch { }
        }

        public void RemovePurple(Bitmap srcBitmap)
        {
            byte a = 0;
            byte r = 0;
            byte g = 0;
            byte b = 0;

            try
            {
                BitmapData srcData = srcBitmap.LockBits(new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* srcPixels = (byte*)srcData.Scan0;

                    for (int x = 0; x < srcBitmap.Width; x++)
                    {
                        for (int y = 0; y < srcBitmap.Height; y++)
                        {
                            byte* srcOffset = srcPixels + (x * 4) + (y * srcData.Stride);

                            r = *(srcOffset);
                            g = *(srcOffset + 1);
                            b = *(srcOffset + 2);
                            a = *(srcOffset + 3);

                            if ((a == 255) && (r == 255) && (b == 255) && (g == 0))
                            {
                                if (*(srcOffset + 1) == 0)
                                {
                                    *(srcOffset + 3) = 0;
                                }

                                *(srcOffset) = 0;
                                *(srcOffset + 1) = 0;
                                *(srcOffset + 2) = 0;
                            }
                        }
                    }
                }

                srcBitmap.UnlockBits(srcData);
            }
            catch { }
        }

        public void Add(string textureName)
        {
            string srcFile = GetSourcePath(textureName);
            string destFile = Path.Combine(DestPath, textureName) + ".png";

            Bitmap original = (Bitmap)Bitmap.FromFile(srcFile);

            SaveAndManifest(original, destFile);
        }

        public void AddWithShadow(string textureName)
        {
            string srcFile = GetSourcePath(textureName);
            string destFile = Path.Combine(DestPath, textureName) + ".png";

            if (imageSheetGroupName != null)
            {
                if (!ForceRegen)
                {
                    string tmpFile = Path.Combine(tmpDir, textureName) + ".png";

                    if (File.Exists(tmpFile) && (new FileInfo(srcFile).LastWriteTime < new FileInfo(tmpFile).LastWriteTime))
                    {
                        imageSheetGroupImages[destFile] = new ImageSheetEntry { Bitmap = new Bitmap(tmpFile), NamePrefix = ImageNamePrefix };

                        return;
                    }
                }
            }
            else
            {
                if (!ForceRegen && !IsNewer(srcFile, destFile))
                    return;
            }

            Bitmap original;

            original = (Bitmap)Bitmap.FromFile(srcFile);

            int imageWidth = original.Width;
            int imageHeight = original.Height;

            Bitmap newBitmap = new Bitmap(original.Width, (original.Height + (VerticalPadding * 2)));

            AddShadow(original, newBitmap, new Rectangle(0, 0, imageWidth, imageHeight), new Point(0, VerticalPadding));

            SaveAndManifest(newBitmap, destFile);
        }

        public void AddShadow(Bitmap srcBitmap, Bitmap destBitmap, Rectangle srcRect, Point destOffset)
        {
            AddShadow(srcBitmap, destBitmap, Graphics.FromImage(destBitmap), srcRect, destOffset);
        }

        public void AddShadow(Bitmap srcBitmap, Bitmap destBitmap, Graphics destRenderer, Rectangle srcRect, Point destOffset)
        {
            Bitmap blurredImage = new Bitmap(srcRect.Width, srcRect.Height);
            GaussianBlur blur = new GaussianBlur(3, 1.0f / 1.5f);

            destRenderer.DrawImage(srcBitmap, srcRect);

            ToBlack(destBitmap);

            blur.Apply(destBitmap, blurredImage);

            destRenderer.DrawImage(blurredImage, new Rectangle(destOffset.X, destOffset.Y, srcRect.Width, srcRect.Height), new Rectangle(0, 0, srcRect.Width, srcRect.Height), GraphicsUnit.Pixel);
            destRenderer.DrawImage(srcBitmap, new Rectangle(destOffset.X, destOffset.Y, srcRect.Width, srcRect.Height), new Rectangle(0, 0, srcRect.Width, srcRect.Height), GraphicsUnit.Pixel);
        }

        public void FillVertical(Bitmap bitmap, int padding)
        {
            for (int offset = 0; offset < padding; offset++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, offset, bitmap.GetPixel(x, padding));

                    bitmap.SetPixel(x, bitmap.Height - (offset + 1), bitmap.GetPixel(x, bitmap.Height - (padding + 1)));
                }
            }
        }

        Graphics measureGraphics = Graphics.FromImage(new Bitmap(1, 1, PixelFormat.Format32bppArgb));

        void MeasureString(string str, Font font, out int width, out int height)
        {
            SizeF sizeWidth = measureGraphics.MeasureString(str, font);
            SizeF size = measureGraphics.MeasureString(str, font, PointF.Empty, StringFormat.GenericTypographic);

            size.Width = sizeWidth.Width;

            width = (int)Math.Ceiling(size.Width);
            height = (int)Math.Ceiling(size.Height);
        }

        public void AddFont(string name, string fontFamily, float emSize)
        {
            AddFont(name, fontFamily, FontStyle.Regular, emSize);
        }

        public void AddFont(string name, string fontFamily, FontStyle fontStyle, float emSize)
        {
            Font font = new Font(fontFamily, emSize, fontStyle);

            AddFont(name, font, 0x20, 0x7f, 16, antialias: true);
        }

        public void AddFont(string name, Font font, int minChar, int maxChar, int glphsPerLine, bool antialias)
        {
            string destFile = Path.Combine(DestPath, name) + ".png";

            List<Bitmap> bitmaps = new List<Bitmap>();
            List<Rectangle> cropRects = new List<Rectangle>();
            List<int> xPositions = new List<int>();
            List<int> yPositions = new List<int>();

            const int padding = 8;

            int width = padding;
            int height = padding;
            int lineWidth = padding;
            int lineHeight = padding;
            int count = 0;

            for (char ch = (char)minChar; ch < maxChar; ch++)
            {
                Bitmap charBitmap = RasterizeCharacter(ch, font, antialias);

                Rectangle cropRect = CropCharacter(charBitmap);

                bitmaps.Add(charBitmap);

                xPositions.Add(lineWidth);
                yPositions.Add(height);
                cropRects.Add(cropRect);

                lineWidth += cropRect.Width + padding;
                lineHeight = Math.Max(lineHeight, cropRect.Height + padding);

                count++;

                if ((count == glphsPerLine) || (ch == maxChar - 1))
                {
                    width = Math.Max(width, lineWidth);
                    height += lineHeight;
                    lineWidth = padding;
                    lineHeight = padding;
                    count = 0;
                }
            }

            SpriteFontDefinition fontEntry = new SpriteFontDefinition();

            fontEntry.Name = name;
            fontEntry.Glyphs = new SpriteFontGlyph[cropRects.Count];

            for (int i = 0; i < cropRects.Count; i++)
            {
                fontEntry.Glyphs[i].Character = (ushort)(minChar + i);
                fontEntry.Glyphs[i].X = xPositions[i];
                fontEntry.Glyphs[i].Y = yPositions[i];
                fontEntry.Glyphs[i].Width = cropRects[i].Width;
                fontEntry.Glyphs[i].Height = cropRects[i].Height;
            }

            imageSheet.Fonts.Add(fontEntry);

            Bitmap shadowed = null;

            using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (Graphics outputGraphics = Graphics.FromImage(bitmap))
                {
                    outputGraphics.CompositingMode = CompositingMode.SourceCopy;

                    for (int i = 0; i < bitmaps.Count; i++)
                    {
                        outputGraphics.DrawImage(bitmaps[i], new Rectangle(xPositions[i], yPositions[i], cropRects[i].Width, cropRects[i].Height), cropRects[i], GraphicsUnit.Pixel);
                    }

                    outputGraphics.Flush();
                }

                shadowed = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                AddShadow(bitmap, shadowed, new Rectangle(0, 0, width, height), new Point(0, 0));

                SaveAndManifest(shadowed, destFile);
            }

            // Clean up temporary objects.
            foreach (Bitmap bitmap in bitmaps)
                bitmap.Dispose();
        }

        private Bitmap RasterizeCharacter(char ch, Font font, bool antialias)
        {
            int width;
            int height;

            string text = ch.ToString();

            MeasureString(text, font, out width, out height);

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (Graphics bitmapGraphics = Graphics.FromImage(bitmap))
            {
                if (antialias)
                {
                    bitmapGraphics.TextRenderingHint =
                        TextRenderingHint.AntiAliasGridFit;
                }
                else
                {
                    bitmapGraphics.TextRenderingHint =
                        TextRenderingHint.SingleBitPerPixelGridFit;
                }

                bitmapGraphics.Clear(Color.Transparent);

                using (Brush brush = new SolidBrush(Color.White))
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Near;

                    bitmapGraphics.DrawString(text, font, brush, 0, 0, format);
                }

                bitmapGraphics.Flush();
            }

            return bitmap;
        }

        private static Rectangle CropCharacter(Bitmap bitmap)
        {
            int cropLeft = 0;
            int cropRight = bitmap.Width - 1;

            // Remove unused space from the left.
            while ((cropLeft < cropRight) && (BitmapIsEmpty(bitmap, cropLeft)))
                cropLeft++;

            // Remove unused space from the right.
            while ((cropRight > cropLeft) && (BitmapIsEmpty(bitmap, cropRight)))
                cropRight--;

            // Don't crop if that would reduce the glyph down to nothing at all!
            if (cropLeft == cropRight)
                return new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            // Add some padding back in.
            cropLeft = Math.Max(cropLeft - 1, 0);
            cropRight = Math.Min(cropRight + 1, bitmap.Width - 1);

            int width = cropRight - cropLeft + 1;

            return new Rectangle(cropLeft, 0, width, bitmap.Height);
        }

        private static bool BitmapIsEmpty(Bitmap bitmap, int x)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y).A != 0)
                    return false;
            }

            return true;
        }

        public void SaveAndManifest(Bitmap bitmap, string destFile)
        {
            if (imageSheetGroupName != null)
            {
                imageSheetGroupImages[destFile] = new ImageSheetEntry { Bitmap = bitmap, NamePrefix = ImageNamePrefix };

                string tmpFile = Path.Combine(tmpDir, Path.GetFileName(destFile));

                bitmap.Save(tmpFile);

                return;
            }

            int usedWidth = bitmap.Width;
            int usedHeight = bitmap.Height;

            if (PadToPowerOfTwo)
            {
                int newWidth = UpperPowerOfTwo(bitmap.Width);
                int newHeight = UpperPowerOfTwo(bitmap.Height);

                if ((newWidth != bitmap.Width) || (newHeight != bitmap.Height))
                {
                    Bitmap padded = new Bitmap(newWidth, newHeight);
                    Graphics paddedImageRenderer = Graphics.FromImage(padded);

                    paddedImageRenderer.DrawImage(bitmap, 0, 0, usedWidth, usedHeight);

                    bitmap.Dispose();
                    bitmap = padded;
                }
            }

            SaveImage(bitmap, destFile);

            bitmap.Dispose();
        }

        public void DeleteFile(string textureName)
        {
            string destFile = Path.Combine(DestPath, textureName) + ".png";

            File.Delete(destFile);
        }

        public int UpperPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return v;
        }

        public void SaveImage(Bitmap image, string filename)
        {
            image.Save(filename, ImageFormat.Png);
        }

        public bool IsNewer(string file1, string file2)
        {
            if (!File.Exists(file2))
                return true;

            return !(new FileInfo(file1).LastWriteTime < new FileInfo(file2).LastWriteTime);
        }

    }
}
