using Client.Crafting;
using Client.Items;
using FluentAssertions;

namespace Client.Tests;

public class CraftingGridTests
{
    private static readonly CraftingRecipe[] Recipes =
    [
        new()
        {
            Id = "1",
            Result = "stick",
            Amount = 4,
            Components = [
                ["plank"],
                ["plank"]
            ]
        },
        new()
        {
            Id = "2",
            Result = "pickaxe",
            Amount = 1,
            Components = [
                ["plank", "plank", "plank"],
                [null, "stick", null],
                [null, "stick", null],
            ]
        }
    ];
    
    [Test]
    public void CraftingResult_BasicCrafting_IsCorrect()
    {
        var craftingGrid = new CraftingGrid(3, 3, Recipes);
        
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 0, 0);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 0, 1);

        craftingGrid.Result.Should().NotBeNull();
        craftingGrid.Result.Value.ItemId.Should().Be("stick");
        craftingGrid.Result.Value.Amount.Should().Be(4);
    }
    
    [Test]
    public void CraftingResult_NoValidRecipe_HasNoResult()
    {
        var craftingGrid = new CraftingGrid(3, 3, Recipes);
        
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 0, 0);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 0, 1);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 0, 2);

        craftingGrid.Result.Should().BeNull();
    }
    
    [Test]
    public void CraftingResult_WithSpecificCraftingShape_IsCorrect()
    {
        var craftingGrid = new CraftingGrid(3, 3, Recipes);
        
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 0, 0);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 1, 0);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 2, 0);
        craftingGrid.AddItemStack(new ItemStack("stick", 1), 1, 1);
        craftingGrid.AddItemStack(new ItemStack("stick", 1), 1, 2);

        craftingGrid.Result.Should().NotBeNull();
        craftingGrid.Result.Value.ItemId.Should().Be("pickaxe");
        craftingGrid.Result.Value.Amount.Should().Be(1);
    }
    
    [Test]
    public void CraftingResult_InLargeGrid_IsCorrect()
    {
        var craftingGrid = new CraftingGrid(6, 6, Recipes);
        
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 2, 2);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 3, 2);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 4, 2);
        craftingGrid.AddItemStack(new ItemStack("stick", 1), 3, 3);
        craftingGrid.AddItemStack(new ItemStack("stick", 1), 3, 4);

        craftingGrid.Result.Should().NotBeNull();
        craftingGrid.Result.Value.ItemId.Should().Be("pickaxe");
        craftingGrid.Result.Value.Amount.Should().Be(1);
    }
    
    [Test]
    public void CraftingResult_WithMultipleRecipes_CreatesNothing()
    {
        var craftingGrid = new CraftingGrid(6, 6, Recipes);
        
        // pickaxe
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 2, 2);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 3, 2);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 4, 2);
        craftingGrid.AddItemStack(new ItemStack("stick", 1), 3, 3);
        craftingGrid.AddItemStack(new ItemStack("stick", 1), 3, 4);
        
        // stick
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 0, 0);
        craftingGrid.AddItemStack(new ItemStack("plank", 1), 0, 1);

        craftingGrid.Result.Should().BeNull();
    }
}