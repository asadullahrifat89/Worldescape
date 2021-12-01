using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Worldescape.Assets.Generator
{
    public class ImageTileGenerator
    {
        private Image sourceImage;
        private Size tileSize;

        int rows;
        int columns;

        public ImageTileGenerator(string inputFile, int xSize, int ySize, int rows, int columns)
        {
            if (!File.Exists(inputFile)) throw new FileNotFoundException();

            sourceImage = Image.FromFile(inputFile);
            tileSize = new Size(xSize, ySize);

            this.rows = rows;
            this.columns = columns;
        }

        public void GenerateTiles(string outputPath, string outputFilePrefix)
        {
            int tileWidth = tileSize.Width;
            int tileHeight = tileSize.Height;

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    string outputFileName = Path.Combine(outputPath, $"{outputFilePrefix}_{x}_{y}.png");

                    Rectangle tileBounds = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                    Bitmap target = new Bitmap(tileWidth, tileHeight);

                    using (Graphics graphics = Graphics.FromImage(target))
                    {
                        graphics.DrawImage(
                            sourceImage,
                            new Rectangle(0, 0, tileWidth, tileHeight),
                            tileBounds,
                            GraphicsUnit.Pixel);
                    }

                    target.Save(outputFileName, ImageFormat.Png);
                }
            }
        }
    }
}
