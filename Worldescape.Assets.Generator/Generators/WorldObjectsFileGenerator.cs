using Worldescape.Common;

namespace Worldescape.Assets.Generator
{
    public static class WorldObjectsFileGenerator
    {
        public static void GenerateMsAppxWorld_Objects(string assetSourceLocation, string outputFileLocation)
        {
            // Generate assets
            var host = "ms-appx:///Images/World_Objects";
            List<ConstructAsset> constructs = new List<ConstructAsset>();

            DirectoryInfo parentDirectory = new(assetSourceLocation);

            if (parentDirectory.Exists)
            {
                DirectoryInfo[] categoryDirectories = parentDirectory.GetDirectories();

                foreach (var categoryDirectory in categoryDirectories)
                {
                    DirectoryInfo[] subcategoryDirectories = categoryDirectory.GetDirectories();

                    foreach (var subcategoryDirectory in subcategoryDirectories)
                    {
                        FileInfo[] files = subcategoryDirectory.GetFiles();

                        foreach (FileInfo file in files)
                        {
                            var url = $"{host}/{categoryDirectory.Name}/{subcategoryDirectory.Name}/{file.Name}";

                            ConstructAsset construct = MakeConstructAsset(categoryDirectory, subcategoryDirectory, file, url);

                            constructs.Add(construct);
                        }
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
            List<ConstructAsset> constructs = new List<ConstructAsset>();

            DirectoryInfo parentDirectory = new(assetSourceLocation);

            if (parentDirectory.Exists)
            {
                DirectoryInfo[] categoryDirectories = parentDirectory.GetDirectories();

                foreach (var categoryDirectory in categoryDirectories)
                {
                    DirectoryInfo[] subcategoryDirectories = categoryDirectory.GetDirectories();

                    foreach (var subcategoryDirectory in subcategoryDirectories)
                    {
                        FileInfo[] files = subcategoryDirectory.GetFiles();

                        foreach (FileInfo file in files)
                        {
                            var url = $"{host}\\{categoryDirectory.Name}\\{subcategoryDirectory.Name}\\{file.Name}";

                            ConstructAsset construct = MakeConstructAsset(categoryDirectory, subcategoryDirectory, file, url);

                            constructs.Add(construct);
                        }
                    }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(constructs, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });

                File.WriteAllText(outputFileLocation, json);
            }
        }

        private static ConstructAsset MakeConstructAsset(DirectoryInfo categoryDirectory, DirectoryInfo subcategoryDirectory, FileInfo file, string url)
        {
            return new ConstructAsset()
            {
                Category = categoryDirectory.Name,
                SubCategory = $"{categoryDirectory.Name}\\{subcategoryDirectory.Name}",
                Name = Constants.CamelToName(file.Name).Replace(".png", ""),
                ImageUrl = url,
            };
        }
    }
}
