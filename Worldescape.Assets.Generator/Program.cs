// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Worldescape.Common;

var executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;

Console.WriteLine("Welcom to Worldescape Asset Generator!");
Console.WriteLine("What would you like to do?");
Console.WriteLine("1. Generate Assets (ms-appx)");
Console.WriteLine("2. Generate Assets (web-http)");

var choice = Console.ReadLine();

var assetSourceLocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets\\Assets\\World_Objects");
var outputFileLocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets.Generator\\World_Objects.json");

switch (choice)
{
    case "1":
        {
            // Generate assets
            var host = "ms-appx:///Images/World_Objects";            

            DirectoryInfo parentDirectory = new(assetSourceLocation);

            List<ConstructAsset> constructs = new List<ConstructAsset>();

            if (parentDirectory.Exists)
            {
                DirectoryInfo[] directories = parentDirectory.GetDirectories();

                foreach (var directory in directories)
                {
                    FileInfo[] files = directory.GetFiles();

                    foreach (FileInfo file in files)
                    {
                        var url = $"{host}/{directory.Name}/{file.Name}";

                        var construct = new ConstructAsset()
                        {
                            Category = Constants.CamelToName(directory.Name),
                            Name = Constants.CamelToName(file.Name).Replace(".png", ""),
                            ImageUrl = url,
                        };

                        constructs.Add(construct);
                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(constructs, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

                File.WriteAllText(outputFileLocation, json);                
            }
        }
        break;
    case "2":
        {
            // Generate assets
            var host = "World_Objects";            

            DirectoryInfo parentDirectory = new(assetSourceLocation);

            List<ConstructAsset> constructs = new List<ConstructAsset>();

            if (parentDirectory.Exists)
            {
                DirectoryInfo[] directories = parentDirectory.GetDirectories();

                foreach (var directory in directories)
                {
                    FileInfo[] files = directory.GetFiles();

                    foreach (FileInfo file in files)
                    {
                        var url = $"{host}\\{directory.Name}\\{file.Name}";

                        var construct = new ConstructAsset()
                        {
                            Category = Constants.CamelToName(directory.Name),
                            Name = Constants.CamelToName(file.Name).Replace(".png", ""),
                            ImageUrl = url,
                        };

                        constructs.Add(construct);
                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(constructs, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

                File.WriteAllText(outputFileLocation, json);             
            }
        }
        break;
    default:
        break;
}

Console.WriteLine("==========================");
Console.WriteLine($"{outputFileLocation} file has been generated.");

Console.ReadLine();