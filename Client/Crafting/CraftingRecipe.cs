namespace Client.Crafting;

public class CraftingRecipe
{
    public string Id { get; set; }
    public string Result { get; set; }
    public int Amount { get; set; }
    public string[][] Components { get; set; } 
}