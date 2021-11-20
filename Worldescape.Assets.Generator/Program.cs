// See https://aka.ms/new-console-template for more information

using LiteDB;
using Worldescape.Data;

var executingAssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

Console.WriteLine("Welcom to Worldescape Asset Generator!");
Console.WriteLine("What would you like to do?");
Console.WriteLine("1. Generate Assets (ms-appx)");
Console.WriteLine("2. Generate Assets (http)");

var choice = Console.ReadLine();

switch (choice)
{
    case "1":
        {
            // Generate assets
            var host = "ms-appx:///Images/World_Objects";

            var newlocation = executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape.Assets\\Assets\\World_Objects");

            //DirectoryInfo parentDirectory = new(executingAssemblyLocation.Replace("Worldescape.Assets.Generator\\bin\\Debug\\net6.0\\Worldescape.Assets.Generator.dll", "Worldescape\\Worldescape\\Images\\World_Objects"));
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
                            Category = directory.Name.Replace("_", " "),
                            Name = file.Name.Replace("_", " ").Replace(".png", ""),
                            ImageUrl = url,
                        };

                        constructs.Add(construct);
                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(constructs, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

                Console.WriteLine("==========================");

                Console.WriteLine(json);

                Console.WriteLine("==========================");
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
                            Category = directory.Name.Replace("_", " "),
                            Name = file.Name.Replace("_", " ").Replace(".png", ""),
                            ImageUrl = url,
                        };

                        constructs.Add(construct);
                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(constructs, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

                Console.WriteLine("==========================");

                Console.WriteLine(json);

                Console.WriteLine("==========================");
            }
        }
        break;
    default:
        break;
}

Console.ReadLine();


// Test database

//using (var db = new LiteDatabase(@"Test.db"))
//{
//    // Get Avatars collection
//    var colAvatars = db.GetCollection<Avatar>("Avatars");

//    var avatar = new Avatar()
//    {
//        Id = 112,
//        Name = "Test"
//    };

//    BsonValue? id = colAvatars.Insert(avatar);
//}