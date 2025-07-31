using Client.Items;

namespace Client.Crafting;

public class CraftingGrid
{
    public int Width => _width;
    public int Height => _height;
    
    public struct CraftingResult
    {
        public string ItemId { get; set; }
        public int Amount { get; set; }
    }
    
    public CraftingResult? Result { get; private set; }
    
    private readonly int _width;
    private readonly int _height;
    private readonly CraftingRecipe[] _craftingRecipes;

    public class Slot
    {
        public ItemStack? Item { get; set; }
    }

    private readonly Slot[] _slots;
    
    public CraftingGrid(int width, int height, CraftingRecipe[] craftingRecipes)
    {
        _width = width;
        _height = height;
        _craftingRecipes = craftingRecipes;
        _slots = new Slot[width * height];
        for (var i = 0; i < width * height; i++)
        {
            _slots[i] = new Slot();
        }
    }

    public Slot[] GetAllSlots()
    {
        return _slots;
    }

    public void FinalizeCrafting()
    {
        for (var i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];
            if (slot.Item.HasValue)
            {
                slot.Item = new ItemStack(slot.Item.Value.ItemId, slot.Item.Value.Amount - 1);
                if (slot.Item.Value.Amount == 0)
                {
                    slot.Item = null;
                }
            }
        }
        UpdateCraftingResult();
    }

    public void AddItemStack(ItemStack itemStack, int x, int y)
    {
        var index = GridPositionToIndex(x, y);
        var slot = _slots[index];
        if (slot.Item.HasValue)
        {
            if (slot.Item.Value.ItemId == itemStack.ItemId)
            {
                itemStack = new ItemStack(itemStack.ItemId, slot.Item.Value.Amount + itemStack.Amount);
            }
        }
        
        slot.Item = itemStack;
        
        UpdateCraftingResult();
    }

    public ItemStack? GetStackAtSlot(int x, int y)
    {
        var index = GridPositionToIndex(x, y);
        var slot = _slots[index];
        return slot.Item;
    }

    public void ClearStack(int x, int y)
    {
        var index = GridPositionToIndex(x, y);
        _slots[index].Item = null;
    }

    public void ClearGrid()
    {
        foreach (var slot in _slots)
        {
            slot.Item = null;
        }
    }

    private void UpdateCraftingResult()
    {
        var nonEmptySlots = GetNonEmptySlots();
        if (nonEmptySlots.Count == 0)
        {
            Result = null;
            return;
        }

        // Find bounding rectangle of items
        var (minX, minY, maxX, maxY) = GetBoundingRect(nonEmptySlots);
        var activeWidth = maxX - minX + 1;
        var activeHeight = maxY - minY + 1;

        // Try each recipe
        foreach (var recipe in _craftingRecipes)
        {
            if (TryMatchRecipe(recipe, minX, minY, activeWidth, activeHeight))
            {
                Result = new CraftingResult 
                { 
                    ItemId = recipe.Result, 
                    Amount = recipe.Amount 
                };
                return;
            }
        }

        Result = null; 
    }
    
    private bool TryMatchRecipe(CraftingRecipe recipe, int startX, int startY, int activeWidth, int activeHeight)
    {
        var recipeHeight = recipe.Components.Length;
        var recipeWidth = recipe.Components[0].Length;

        // Check if recipe has null values (requires exact positioning)
        bool hasNulls = recipe.Components.Any(row => row.Any(item => item == "null"));

        if (hasNulls)
        {
            // Exact match required - dimensions must match
            if (activeWidth != recipeWidth || activeHeight != recipeHeight)
                return false;
        
            return MatchesExactPattern(recipe, startX, startY);
        }
        else
        {
            // Flexible positioning - try all valid positions in grid
            for (int offsetX = 0; offsetX <= _width - recipeWidth; offsetX++)
            {
                for (int offsetY = 0; offsetY <= _height - recipeHeight; offsetY++)
                {
                    if (MatchesFlexiblePattern(recipe, offsetX, offsetY))
                        return true;
                }
            }
            return false;
        }
    } 
    
    private bool MatchesExactPattern(CraftingRecipe recipe, int startX, int startY)
    {
        for (int y = 0; y < recipe.Components.Length; y++)
        {
            for (int x = 0; x < recipe.Components[y].Length; x++)
            {
                var expectedItem = recipe.Components[y][x];
                var actualItem = GetItemAt(startX + x, startY + y);

                if (expectedItem == "null")
                {
                    if (actualItem != null) return false;
                }
                else
                {
                    if (actualItem?.ItemId != expectedItem) return false;
                }
            }
        }
        return true;
    } 
    
    private bool MatchesFlexiblePattern(CraftingRecipe recipe, int offsetX, int offsetY)
    {
        // First check if pattern matches at this position
        for (int y = 0; y < recipe.Components.Length; y++)
        {
            for (int x = 0; x < recipe.Components[y].Length; x++)
            {
                var expectedItem = recipe.Components[y][x];
                var actualItem = GetItemAt(offsetX + x, offsetY + y);
            
                if (actualItem?.ItemId != expectedItem) return false;
            }
        }

        // Ensure no extra items outside the pattern
        return GetNonEmptySlots().All(slot => 
            slot.x >= offsetX && slot.x < offsetX + recipe.Components[0].Length &&
            slot.y >= offsetY && slot.y < offsetY + recipe.Components.Length);
    } 
    
    private List<(int x, int y)> GetNonEmptySlots()
    {
        var slots = new List<(int x, int y)>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (GetItemAt(x, y) != null)
                    slots.Add((x, y));
            }
        }
        return slots;
    } 
    
    private ItemStack? GetItemAt(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return null;
        return _slots[GridPositionToIndex(x, y)]?.Item;
    } 
    
    private (int minX, int minY, int maxX, int maxY) GetBoundingRect(List<(int x, int y)> slots)
    {
        return (
            slots.Min(s => s.x),
            slots.Min(s => s.y), 
            slots.Max(s => s.x),
            slots.Max(s => s.y)
        );
    } 

    private int GridPositionToIndex(int x, int y)
    {
        return x + (y * _height);
    }
}