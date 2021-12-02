// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Worldescape.Assets.Generator;

var executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
var assetSourceLocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets\\Assets\\World_Objects");
var outputFileLocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets.Generator\\World_Objects.json");

Console.WriteLine("Welcom to Worldescape Asset Generator!");
Console.WriteLine("What would you like to do?");
Console.WriteLine("1. Generate Assets (ms-appx)");
Console.WriteLine("2. Generate Assets (web-http)");
Console.WriteLine("3. Generate Tiles");

var choice = Console.ReadLine();

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
            Console.WriteLine("Enter InputFilePath ~ TileSize(256x192) ~ RowsColumnsInInputFile(3x10) ~ OutputPath ~ OutputPrefix");
            var input = Console.ReadLine();
            
            if (input == null)
                return;

            var parts = input.Split('~');

            var inputFile = parts[0].Trim('"', ' ');
            var size = parts[1].Trim(' ');
            var rowCols = parts[2].Trim(' ');
            var outputPath = parts[3].Trim('"', ' ');
            var outputFilePrefix = parts[4].Trim('"', ' ');

            parts = size.Split('x');
            var x = Convert.ToInt32(parts[0]);
            var y = Convert.ToInt32(parts[1]);

            parts = rowCols.Split('x');
            var row = Convert.ToInt32(parts[0]);
            var column = Convert.ToInt32(parts[1]);

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