// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Worldescape.Assets.Generator;
using Worldescape.Common;

var executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;

Console.WriteLine("Welcom to Worldescape Asset Generator!");
Console.WriteLine("What would you like to do?");
Console.WriteLine("1. Generate Assets (ms-appx)");
Console.WriteLine("2. Generate Assets (web-http)");
Console.WriteLine("3. Generate Tiles");

var choice = Console.ReadLine();

var assetSourceLocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets\\Assets\\World_Objects");
var outputFileLocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets.Generator\\World_Objects.json");

switch (choice)
{
    case "1":
        {
            WorldObjectsFileGenerator.GenerateMsAppxWorld_Objects(assetSourceLocation, outputFileLocation);

            Console.WriteLine("==========================");
            Console.WriteLine($"{outputFileLocation} file has been generated.");
        }
        break;
    case "2":
        {
            WorldObjectsFileGenerator.GenerateWebHttpWorld_Objects(assetSourceLocation, outputFileLocation);

            Console.WriteLine("==========================");
            Console.WriteLine($"{outputFileLocation} file has been generated.");
        }
        break;
    case "3":
        {
            Console.WriteLine("Enter source image file Path:");
            var inputFile = Console.ReadLine();
            inputFile = inputFile.Trim('"');

            Console.WriteLine("Enter tile size: i.e 256x192");
            var size = Console.ReadLine();

            if (size == null)
                return;

            var parts = size.Split('x');

            var x = Convert.ToInt32(parts[0]);
            var y = Convert.ToInt32( parts[1]);

            Console.WriteLine("Enter number of tiles in source image in rowxcolumn: i.e 3x10");
            var rowCols = Console.ReadLine();

            parts = rowCols.Split('x');

            var row = Convert.ToInt32(parts[0]);
            var column = Convert.ToInt32(parts[1]);

            Console.WriteLine("Enter output path:");
            var outputPath = Console.ReadLine();
            outputPath = outputPath.Trim('"');

            Console.WriteLine("Enter output file name prefix:");
            var outputFilePrefix = Console.ReadLine();
            outputFilePrefix = outputFilePrefix.Trim('"');

            if (inputFile != null && outputPath != null)
            {
                var tiler = new ImageTileGenerator(inputFile: inputFile, xSize: x, ySize: y, rows: row, columns: column);
                tiler.GenerateTiles(outputPath: outputPath, outputFilePrefix: outputFilePrefix);
            }

            Console.WriteLine("==========================");
            Console.WriteLine($"{outputPath} files have been generated.");
        }
        break;
    default:
        break;
}

Console.ReadLine();