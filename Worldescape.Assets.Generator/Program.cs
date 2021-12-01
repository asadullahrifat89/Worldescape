// See https://aka.ms/new-console-template for more information

using Worldescape.Common;

var executingAssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

Console.WriteLine("Welcom to Worldescape Asset Generator!");
Console.WriteLine("What would you like to do?");
Console.WriteLine("1. Generate Assets (ms-appx)");
Console.WriteLine("2. Generate Assets (web-http)");

var choice = Console.ReadLine();

switch (choice)
{
    case "1":
        {
            // Generate assets
            var host = "ms-appx:///Images/World_Objects";

            var newlocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets\\Assets\\World_Objects");

            DirectoryInfo parentDirectory = new(newlocation);

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

                File.WriteAllText("World_Objects.json", json);

                Console.WriteLine("==========================");
                Console.WriteLine("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\World_Objects.json file has been generated.");
            }
        }
        break;
    case "2":
        {
            // Generate assets
            var host = "World_Objects";

            var newlocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets\\Assets\\World_Objects");

            DirectoryInfo parentDirectory = new(newlocation);

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

                File.WriteAllText("World_Objects.json", json);

                Console.WriteLine("==========================");
                Console.WriteLine("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\World_Objects.json file has been generated.");                
            }
        }
        break;
    default:
        break;
}

Console.ReadLine();