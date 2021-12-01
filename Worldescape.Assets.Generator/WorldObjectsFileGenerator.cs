using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Common;

namespace Worldescape.Assets.Generator
{
    public static class WorldObjectsFileGenerator
    {
        public static void GenerateMsAppxWorld_Objects(string assetSourceLocation, string outputFileLocation) 
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

        public static void GenerateWebHttpWorld_Objects(string assetSourceLocation, string outputFileLocation) 
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
    }
}
