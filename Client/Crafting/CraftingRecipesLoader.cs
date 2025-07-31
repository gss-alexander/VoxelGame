using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Client.Crafting;

public static class CraftingRecipesLoader
{
    private static readonly string FilePath = Path.Combine("..", "..", "..", "Resources", "Data", "crafting_recipes.yaml");

    private class CraftingRecipeConfig
    {
        public Dictionary<string, CraftingRecipe> Recipes { get; set; } = new();
    }

    public static CraftingRecipe[] Load()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var fileContent = File.ReadAllText(FilePath);
        var config = deserializer.Deserialize<CraftingRecipeConfig>(fileContent);

        var recipes = new List<CraftingRecipe>();
        foreach (var kvp in config.Recipes)
        {
            var (id, recipe) = kvp;
            recipe.Id = id;
            recipes.Add(recipe);
            Console.WriteLine(recipe);
        }

        return recipes.ToArray();
    }
}