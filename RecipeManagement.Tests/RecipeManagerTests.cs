using System.Collections.Generic;
using RecipeManagement.Core;

namespace RecipeManagement.Tests;

public sealed class RecipeManagerTests
{
    [Fact]
    public void Constructor_BuildsRecipeDictionary()
    {
        var manager = CreateManager();
        Assert.Equal(2, manager.RecipeCount);
        Assert.Equal("Recipe A", manager.FindRecipe(10)?.Title);
    }

    [Fact]
    public void InstructionsAreCompletedInFileOrder()
    {
        var manager = CreateManager();
        Assert.True(manager.StartCooking(10));
        Assert.Equal("First step", manager.PeekNextInstruction());
        Assert.Equal("First step", manager.CompleteNextInstruction());
        Assert.Equal("Second step", manager.PeekNextInstruction());
    }

    [Fact]
    public void RemovedRecipesAreRestoredLastInFirstOut()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(20);
        Assert.Equal(20, manager.PeekLastRemovedRecipe());
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 20 }, manager.GetCookingPlan());
    }

    [Fact]
    public void Constructor_NullRecipes_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RecipeManager(null!));
    }

    [Fact]
    public void Constructor_DuplicateId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RecipeManager(new[]
        {
            MakeRecipe(10, "Recipe A"),
            MakeRecipe(10, "Duplicate")
        }));
    }

    [Fact]
    public void Constructor_BlankTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RecipeManager(new[]
        {
            MakeRecipe(10, "   ")
        }));
    }

    [Fact]
    public void AddRecipe_ValidNewRecipe_ReturnsTrue()
    {
        var manager = CreateManager();
        bool added = manager.AddRecipe(MakeRecipe(30, "Recipe C"));
        Assert.True(added);
        Assert.Equal(3, manager.RecipeCount);
        Assert.Equal("Recipe C", manager.FindRecipe(30)?.Title);
    }

    [Fact]
    public void AddRecipe_DuplicateId_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.AddRecipe(MakeRecipe(10, "Duplicate A")));
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void AddRecipe_NonPositiveOrBlank_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.AddRecipe(MakeRecipe(0, "No ID")));
        Assert.False(manager.AddRecipe(MakeRecipe(-1, "Negative ID")));
        Assert.False(manager.AddRecipe(MakeRecipe(40, " ")));
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void AddRecipe_NullRecipe_ThrowsArgumentNullException()
    {
        var manager = CreateManager();
        Assert.Throws<ArgumentNullException>(() => manager.AddRecipe(null!));
    }

    [Fact]
    public void FindRecipe_MissingId_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.FindRecipe(999));
    }

    [Fact]
    public void RemoveRecipe_ExistingAndNotPlanned_ReturnsTrue()
    {
        var manager = CreateManager();
        Assert.True(manager.RemoveRecipe(20));
        Assert.Null(manager.FindRecipe(20));
    }

    [Fact]
    public void RemoveRecipe_WhileInCookingPlan_ReturnsFalse()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        Assert.False(manager.RemoveRecipe(10));
        Assert.NotNull(manager.FindRecipe(10));
    }

    [Fact]
    public void RemoveRecipe_MissingId_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.RemoveRecipe(999));
    }

    [Fact]
    public void AddIngredients_CopiesIngredientsInOrder()
    {
        var manager = CreateManager();
        int added = manager.AddIngredientsToShoppingList(10);
        Assert.Equal(1, added);
        Assert.Equal(new[] { "1 apple" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
    }

    [Fact]
    public void AddIngredients_MissingRecipe_ReturnsZero()
    {
        var manager = CreateManager();
        Assert.Equal(0, manager.AddIngredientsToShoppingList(999));
        Assert.Empty(manager.GetShoppingList());
    }

    [Fact]
    public void ClearShoppingList_RemovesAllItems()
    {
        var manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);
        manager.ClearShoppingList();
        Assert.Empty(manager.GetShoppingList());
        Assert.Equal(0, manager.ShoppingItemCount);
    }

    [Fact]
    public void AddPlan_MissingRecipe_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.AddRecipeToCookingPlan(999));
        Assert.Empty(manager.GetCookingPlan());
    }

    [Fact]
    public void AddPlan_DuplicateRecipe_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.False(manager.AddRecipeToCookingPlan(10));
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.CookingPlanCount);
    }

    [Fact]
    public void GetCookingPlan_AppendsAndPreservesOrder()
    {
        var manager = CreateManager();
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.True(manager.AddRecipeToCookingPlan(20));
        Assert.Equal(new[] { 10, 20 }, manager.GetCookingPlan());
    }

    [Fact]
    public void RemovePlan_NotPlanned_DoesNotChangeStack()
    {
        var manager = CreateManager();
        Assert.False(manager.RemoveRecipeFromCookingPlan(10));
        Assert.Null(manager.PeekLastRemovedRecipe());
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RemovePlan_Success_PushesIdToStack()
    {
        var manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        Assert.True(manager.RemoveRecipeFromCookingPlan(10));
        Assert.Equal(10, manager.PeekLastRemovedRecipe());
        Assert.Equal(1, manager.RemovedRecipeCount);
    }

    [Fact]
    public void PeekLastRemoved_Empty_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.PeekLastRemovedRecipe());
    }

    [Fact]
    public void Restore_EmptyStack_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.RestoreLastRemovedRecipe());
    }

    [Fact]
    public void PeekInstruction_EmptyQueue_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.PeekNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void CompleteInstruction_EmptyQueue_ReturnsNull()
    {
        var manager = CreateManager();
        Assert.Null(manager.CompleteNextInstruction());
    }

    [Fact]
    public void StartCooking_MissingRecipe_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.StartCooking(999));
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void StartCooking_WithoutInstructions_ReturnsFalse()
    {
        var manager = CreateManager();
        Assert.False(manager.StartCooking(20));
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void StartCooking_ReplacesPreviousInstructionQueue()
    {
        var manager = ManagerWith(
            MakeRecipe(10, "A", instructions: new() { "Step 1", "Step 2" }),
            MakeRecipe(20, "B", instructions: new() { "Another first" }));

        Assert.True(manager.StartCooking(10));
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.True(manager.StartCooking(20));
        Assert.Equal("Another first", manager.PeekNextInstruction());
        Assert.Equal(1, manager.PendingInstructionCount);
    }

    [Fact]
    public void CatalogueShoppingPlanStack_WorkTogether()
    {
        var manager = ManagerWith(MakeRecipe(10, "A", ingredients: new() { "egg", "flour" }));

        Assert.Equal(2, manager.AddIngredientsToShoppingList(10));
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.True(manager.RemoveRecipeFromCookingPlan(10));
        Assert.True(manager.RestoreLastRemovedRecipe());

        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(new[] { "egg", "flour" }, manager.GetShoppingList());
    }

    private static RecipeManager CreateManager()
    {
        return ManagerWith(
            new Recipe
            {
                Id = 10,
                Title = "Recipe A",
                Ingredients = new() { "1 apple" },
                Instructions = new() { "First step", "Second step" }
            },
            new Recipe
            {
                Id = 20,
                Title = "Recipe B"
            });
    }

    private static RecipeManager ManagerWith(params Recipe[] recipes) => new(recipes);

    private static Recipe MakeRecipe(
        int id,
        string title,
        List<string>? ingredients = null,
        List<string>? instructions = null)
    {
        return new Recipe
        {
            Id = id,
            Title = title,
            Ingredients = ingredients ?? new List<string>(),
            Instructions = instructions ?? new List<string>()
        };
    }
}
