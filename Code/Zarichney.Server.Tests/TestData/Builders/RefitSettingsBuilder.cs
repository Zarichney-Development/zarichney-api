using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Refit;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Test data builder for creating RefitSettings instances with various configurations.
/// Provides a fluent API for constructing test-specific Refit settings.
/// </summary>
public class RefitSettingsBuilder
{
  private RefitSettings _settings;

  public RefitSettingsBuilder()
  {
    _settings = new RefitSettings();
  }

  /// <summary>
  /// Creates a builder with default settings commonly used in tests.
  /// </summary>
  public static RefitSettingsBuilder Default()
  {
    return new RefitSettingsBuilder()
        .WithCollectionFormat(CollectionFormat.Multi)
        .WithBufferHttpContent(false);
  }

  /// <summary>
  /// Creates a builder with minimal settings.
  /// </summary>
  public static RefitSettingsBuilder Minimal()
  {
    return new RefitSettingsBuilder();
  }

  /// <summary>
  /// Creates a builder optimized for integration test scenarios.
  /// </summary>
  public static RefitSettingsBuilder ForIntegrationTests()
  {
    return new RefitSettingsBuilder()
        .WithCollectionFormat(CollectionFormat.Multi)
        .WithBufferHttpContent(true)
        .WithSystemTextJsonContentSerializer();
  }

  /// <summary>
  /// Sets the collection format for query parameters.
  /// </summary>
  public RefitSettingsBuilder WithCollectionFormat(CollectionFormat format)
  {
    _settings.CollectionFormat = format;
    return this;
  }

  /// <summary>
  /// Sets whether to buffer HTTP content (removed in newer Refit versions).
  /// </summary>
  public RefitSettingsBuilder WithBufferHttpContent(bool buffer)
  {
    // BufferHttpContent was removed in newer versions of Refit
    // This method is kept for API compatibility but does nothing
    return this;
  }

  /// <summary>
  /// Configures settings to use System.Text.Json serializer.
  /// </summary>
  public RefitSettingsBuilder WithSystemTextJsonContentSerializer()
  {
    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    _settings.ContentSerializer = new SystemTextJsonContentSerializer(options);
    return this;
  }

  /// <summary>
  /// Configures settings with a custom content serializer.
  /// </summary>
  public RefitSettingsBuilder WithContentSerializer(IHttpContentSerializer serializer)
  {
    _settings.ContentSerializer = serializer;
    return this;
  }

  /// <summary>
  /// Configures settings with a custom URL parameter formatter.
  /// </summary>
  public RefitSettingsBuilder WithUrlParameterFormatter(IUrlParameterFormatter formatter)
  {
    _settings.UrlParameterFormatter = formatter;
    return this;
  }

  /// <summary>
  /// Configures settings with a custom form URL encoded parameter formatter.
  /// </summary>
  public RefitSettingsBuilder WithFormUrlEncodedParameterFormatter(IFormUrlEncodedParameterFormatter formatter)
  {
    _settings.FormUrlEncodedParameterFormatter = formatter;
    return this;
  }

  /// <summary>
  /// Adds an HTTP message handler to the settings.
  /// </summary>
  public RefitSettingsBuilder WithHttpMessageHandlerFactory(Func<HttpMessageHandler> factory)
  {
    _settings.HttpMessageHandlerFactory = factory;
    return this;
  }

  /// <summary>
  /// Sets the exception factory for creating custom exceptions.
  /// </summary>
  public RefitSettingsBuilder WithExceptionFactory(Func<HttpResponseMessage, Task<Exception?>> factory)
  {
    _settings.ExceptionFactory = factory;
    return this;
  }

  /// <summary>
  /// Configures settings for CSV collection format.
  /// </summary>
  public RefitSettingsBuilder WithCsvFormat()
  {
    _settings.CollectionFormat = CollectionFormat.Csv;
    return this;
  }

  /// <summary>
  /// Configures settings for SSV (Space Separated Values) collection format.
  /// </summary>
  public RefitSettingsBuilder WithSsvFormat()
  {
    _settings.CollectionFormat = CollectionFormat.Ssv;
    return this;
  }

  /// <summary>
  /// Configures settings for TSV (Tab Separated Values) collection format.
  /// </summary>
  public RefitSettingsBuilder WithTsvFormat()
  {
    _settings.CollectionFormat = CollectionFormat.Tsv;
    return this;
  }

  /// <summary>
  /// Configures settings for Pipes collection format.
  /// </summary>
  public RefitSettingsBuilder WithPipesFormat()
  {
    _settings.CollectionFormat = CollectionFormat.Pipes;
    return this;
  }

  /// <summary>
  /// Builds the RefitSettings instance with the configured options.
  /// </summary>
  public RefitSettings Build()
  {
    return _settings;
  }

  /// <summary>
  /// Implicit conversion to RefitSettings for convenience.
  /// </summary>
  public static implicit operator RefitSettings(RefitSettingsBuilder builder)
  {
    return builder.Build();
  }
}
