using FluentAssertions;
using Xunit;
using Zarichney.TestingFramework.TestData.Builders;

namespace Zarichney.TestingFramework.Tests.Unit.TestData.Builders;

/// <summary>
/// Unit tests for BaseBuilder abstract class functionality.
/// </summary>
public class BaseBuilderTests
{
    /// <summary>
    /// Concrete implementation of BaseBuilder for testing purposes.
    /// </summary>
    private class TestEntityBuilder : BaseBuilder<TestEntityBuilder, TestEntity>
    {
        public TestEntityBuilder WithId(int id)
        {
            Entity.Id = id;
            return Self();
        }

        public TestEntityBuilder WithName(string name)
        {
            Entity.Name = name;
            return Self();
        }
    }

    /// <summary>
    /// Simple test entity for builder testing.
    /// </summary>
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Build_ShouldReturnNewEntityInstance()
    {
        // Arrange
        var builder = new TestEntityBuilder();

        // Act
        var entity = builder.Build();

        // Assert
        entity.Should().NotBeNull();
        entity.Should().BeOfType<TestEntity>();
    }

    [Fact]
    public void Build_ShouldReturnSameEntityInstanceOnMultipleCalls()
    {
        // Arrange
        var builder = new TestEntityBuilder();

        // Act
        var entity1 = builder.Build();
        var entity2 = builder.Build();

        // Assert
        entity1.Should().BeSameAs(entity2);
    }

    [Fact]
    public void WithMethods_ShouldAllowFluentChaining()
    {
        // Arrange
        var builder = new TestEntityBuilder();
        const int expectedId = 123;
        const string expectedName = "Test Entity";

        // Act
        var entity = builder
            .WithId(expectedId)
            .WithName(expectedName)
            .Build();

        // Assert
        entity.Id.Should().Be(expectedId);
        entity.Name.Should().Be(expectedName);
    }

    [Fact]
    public void Self_ShouldReturnBuilderInstance()
    {
        // Arrange
        var builder = new TestEntityBuilder();

        // Act
        var result = builder.WithId(1);

        // Assert
        result.Should().BeSameAs(builder);
        result.Should().BeOfType<TestEntityBuilder>();
    }

    [Fact]
    public void Entity_ShouldBeInitializedOnConstruction()
    {
        // Arrange & Act
        var builder = new TestEntityBuilder();
        var entity = builder.Build();

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().Be(0); // Default value
        entity.Name.Should().Be(string.Empty); // Default value
    }

    [Fact]
    public void MultipleBuilders_ShouldHaveIndependentEntities()
    {
        // Arrange
        var builder1 = new TestEntityBuilder();
        var builder2 = new TestEntityBuilder();

        // Act
        var entity1 = builder1.WithId(1).WithName("Entity 1").Build();
        var entity2 = builder2.WithId(2).WithName("Entity 2").Build();

        // Assert
        entity1.Should().NotBeSameAs(entity2);
        entity1.Id.Should().Be(1);
        entity1.Name.Should().Be("Entity 1");
        entity2.Id.Should().Be(2);
        entity2.Name.Should().Be("Entity 2");
    }
}