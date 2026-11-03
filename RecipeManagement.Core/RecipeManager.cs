using System;
using System.Collections.Generic;

namespace RecipeManagement.Core;

/// <summary>
/// Implements the Part A recipe catalogue, shopping list, cooking plan,
/// removed-recipe history and instruction queue.
/// </summary>
public sealed class RecipeManager : IRecipeManager
{
    private readonly Dictionary<int, Recipe> _recipes = new();
    private readonly List<string> _shoppingList = new();
    private readonly LinkedList<int> _cookingPlan = new();
    private readonly Stack<int> _removedHistory = new();
    private readonly Queue<string> _pendingInstructions = new();

    public RecipeManager(IEnumerable<Recipe> recipes)
    {
        if (recipes is null)
        {
            throw new ArgumentNullException(nameof(recipes));
        }

        foreach (Recipe recipe in recipes)
        {
            if (recipe is null)
            {
                throw new ArgumentException("A recipe entry must not be null.", nameof(recipes));
            }

            if (recipe.Id <= 0)
            {
                throw new ArgumentException("Recipe ID must be a positive integer.", nameof(recipes));
            }

            if (string.IsNullOrWhiteSpace(recipe.Title))
            {
                throw new ArgumentException("Recipe title must not be blank.", nameof(recipes));
            }

            if (!_recipes.TryAdd(recipe.Id, recipe))
            {
                throw new ArgumentException($"Duplicate recipe ID: {recipe.Id}.", nameof(recipes));
            }
        }
    }

    public int RecipeCount => _recipes.Count;
    public int ShoppingItemCount => _shoppingList.Count;
    public int CookingPlanCount => _cookingPlan.Count;
    public int PendingInstructionCount => _pendingInstructions.Count;
    public int RemovedRecipeCount => _removedHistory.Count;

    public bool AddRecipe(Recipe recipe)
    {
        if (recipe is null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        if (recipe.Id <= 0 ||
            string.IsNullOrWhiteSpace(recipe.Title) ||
            _recipes.ContainsKey(recipe.Id))
        {
            return false;
        }

        _recipes.Add(recipe.Id, recipe);
        return true;
    }

    public Recipe? FindRecipe(int recipeId) =>
        _recipes.TryGetValue(recipeId, out Recipe? recipe) ? recipe : null;

    public bool RemoveRecipe(int recipeId)
    {
        if (!_recipes.ContainsKey(recipeId) || _cookingPlan.Contains(recipeId))
        {
            return false;
        }

        return _recipes.Remove(recipeId);
    }

    public int AddIngredientsToShoppingList(int recipeId)
    {
        if (!_recipes.TryGetValue(recipeId, out Recipe? recipe))
        {
            return 0;
        }

        int added = 0;
        foreach (string ingredient in recipe.Ingredients)
        {
            if (!string.IsNullOrWhiteSpace(ingredient))
            {
                _shoppingList.Add(ingredient);
                added++;
            }
        }

        return added;
    }

    public IReadOnlyList<string> GetShoppingList() => _shoppingList.ToList();

    public void ClearShoppingList() => _shoppingList.Clear();

    public bool AddRecipeToCookingPlan(int recipeId)
    {
        if (!_recipes.ContainsKey(recipeId) || _cookingPlan.Contains(recipeId))
        {
            return false;
        }

        _cookingPlan.AddLast(recipeId);
        return true;
    }

    public bool RemoveRecipeFromCookingPlan(int recipeId)
    {
        if (!_cookingPlan.Remove(recipeId))
        {
            return false;
        }

        _removedHistory.Push(recipeId);
        return true;
    }

    public bool RestoreLastRemovedRecipe()
    {
        if (_removedHistory.Count == 0)
        {
            return false;
        }

        int recipeId = _removedHistory.Pop();
        if (_recipes.ContainsKey(recipeId) && !_cookingPlan.Contains(recipeId))
        {
            _cookingPlan.AddLast(recipeId);
            return true;
        }

        return false;
    }

    public int? PeekLastRemovedRecipe() =>
        _removedHistory.Count > 0 ? _removedHistory.Peek() : null;

    public IReadOnlyList<int> GetCookingPlan() => _cookingPlan.ToList();

    public bool StartCooking(int recipeId)
    {
        if (!_recipes.TryGetValue(recipeId, out Recipe? recipe))
        {
            return false;
        }

        if (recipe.Instructions.Count == 0)
        {
            return false;
        }

        _pendingInstructions.Clear();
        foreach (string instruction in recipe.Instructions)
        {
            _pendingInstructions.Enqueue(instruction);
        }

        return true;
    }

    public string? PeekNextInstruction() =>
        _pendingInstructions.Count > 0 ? _pendingInstructions.Peek() : null;

    public string? CompleteNextInstruction() =>
        _pendingInstructions.Count > 0 ? _pendingInstructions.Dequeue() : null;

    // Part B members remain as supplied stubs until Part B begins.
    public IReadOnlyList<Recipe> SearchByTitle(string searchText) =>
        throw new NotImplementedException("Part B: implement SearchByTitle.");

    public IReadOnlyList<Recipe> SearchByIngredient(string searchText) =>
        throw new NotImplementedException("Part B: implement SearchByIngredient.");

    public IReadOnlyList<Recipe> GetHighestProteinRecipes(int count) =>
        throw new NotImplementedException("Part B: implement GetHighestProteinRecipes.");

    public bool AddSavedRecipe(int recipeId) =>
        throw new NotImplementedException("Part B: implement AddSavedRecipe.");

    public bool RemoveSavedRecipe(int recipeId) =>
        throw new NotImplementedException("Part B: implement RemoveSavedRecipe.");

    public bool IsRecipeSaved(int recipeId) =>
        throw new NotImplementedException("Part B: implement IsRecipeSaved.");

    public IReadOnlyList<int> GetSavedRecipes() =>
        throw new NotImplementedException("Part B: implement GetSavedRecipes.");
}
