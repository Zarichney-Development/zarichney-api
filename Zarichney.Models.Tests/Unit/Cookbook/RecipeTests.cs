using Zarichney.Models.Cookbook;

namespace Zarichney.Models.Tests.Unit.Cookbook;

public class RecipeTests
{
    [Fact]
    public void Recipe_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var recipe = new Recipe
        {
            Title = "Test Recipe",
            Description = "A test recipe description",
            PrepTime = "10 minutes",
            CookTime = "20 minutes",
            Ingredients = ["Ingredient 1", "Ingredient 2"],
            Directions = ["Step 1", "Step 2"]
        };
        var context = new ValidationContext(recipe);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(recipe, context, results, true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void Recipe_WithEmptyTitle_ShouldFailValidation()
    {
        // Arrange
        var recipe = new Recipe
        {
            Title = "",
            Description = "A test recipe description",
            PrepTime = "10 minutes",
            CookTime = "20 minutes",
            Ingredients = ["Ingredient 1"],
            Directions = ["Step 1"]
        };
        var context = new ValidationContext(recipe);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(recipe, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void Recipe_WithTitleTooLong_ShouldFailValidation()
    {
        // Arrange
        var longTitle = new string('A', 201); // Exceeds StringLength(200)
        var recipe = new Recipe
        {
            Title = longTitle,
            Description = "A test recipe description",
            PrepTime = "10 minutes",
            CookTime = "20 minutes",
            Ingredients = ["Ingredient 1"],
            Directions = ["Step 1"]
        };
        var context = new ValidationContext(recipe);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(recipe, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void Recipe_JsonSerialization_ShouldUseCorrectPropertyNames()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = "recipe-123",
            Title = "Test Recipe",
            Description = "Test Description",
            PrepTime = "15 minutes",
            CookTime = "30 minutes",
            Ingredients = ["Salt", "Pepper"],
            Directions = ["Mix", "Cook"]
        };

        // Act
        var json = JsonSerializer.Serialize(recipe);

        // Assert
        json.Should().Contain("\"prep_time\":\"15 minutes\"");
        json.Should().Contain("\"cook_time\":\"30 minutes\"");
        json.Should().Contain("\"Title\":\"Test Recipe\"");
        json.Should().Contain("\"Description\":\"Test Description\"");
    }

    [Fact]
    public void Recipe_JsonDeserialization_ShouldHandleCustomPropertyNames()
    {
        // Arrange
        var json = """
        {
            "Id": "recipe-456",
            "Title": "Deserialized Recipe",
            "Description": "Test Description",
            "prep_time": "20 minutes",
            "cook_time": "45 minutes",
            "Ingredients": ["Flour", "Sugar"],
            "Directions": ["Combine", "Bake"]
        }
        """;

        // Act
        var recipe = JsonSerializer.Deserialize<Recipe>(json);

        // Assert
        recipe.Should().NotBeNull();
        recipe!.Id.Should().Be("recipe-456");
        recipe.Title.Should().Be("Deserialized Recipe");
        recipe.PrepTime.Should().Be("20 minutes");
        recipe.CookTime.Should().Be("45 minutes");
        recipe.Ingredients.Should().BeEquivalentTo(["Flour", "Sugar"]);
        recipe.Directions.Should().BeEquivalentTo(["Combine", "Bake"]);
    }

    [Fact]
    public void Recipe_WithRequiredProperties_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Title = GetRandom.String(),
            Description = GetRandom.String(),
            PrepTime = GetRandom.String(),
            CookTime = GetRandom.String(),
            Ingredients = [GetRandom.String(), GetRandom.String()],
            Directions = [GetRandom.String(), GetRandom.String()]
        };

        // Assert
        recipe.Title.Should().NotBeNullOrEmpty();
        recipe.Description.Should().NotBeNullOrEmpty();
        recipe.PrepTime.Should().NotBeNullOrEmpty();
        recipe.CookTime.Should().NotBeNullOrEmpty();
        recipe.Ingredients.Should().NotBeEmpty();
        recipe.Directions.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("Short")]
    [InlineData("Medium Length Recipe Title")]
    [InlineData("This is a very long recipe title that is still within the 200 character limit and should pass validation without any issues whatsoever")]
    public void Recipe_WithValidTitleLengths_ShouldPassValidation(string title)
    {
        // Arrange
        var recipe = new Recipe
        {
            Title = title,
            Description = "Description",
            PrepTime = "10 min",
            CookTime = "20 min",
            Ingredients = ["Ingredient"],
            Directions = ["Direction"]
        };
        var context = new ValidationContext(recipe);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(recipe, context, results, true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }
}