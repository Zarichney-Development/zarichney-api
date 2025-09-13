using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Zarichney.Tests.Framework.Helpers
{
  /// <summary>
  /// Helper class for resetting static fields during test initialization.
  /// Provides robust reflection utilities for clearing static state to ensure test isolation.
  /// </summary>
  public static class StaticFieldResetHelper
  {
    /// <summary>
    /// Resets a static field to its default value using reflection.
    /// </summary>
    /// <typeparam name="TClass">The type containing the static field</typeparam>
    /// <param name="fieldName">The name of the static field to reset</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>True if the field was successfully reset, false otherwise</returns>
    public static bool ResetStaticField<TClass>(string fieldName, ILogger? logger = null)
    {
      return ResetStaticField(typeof(TClass), fieldName, logger);
    }

    /// <summary>
    /// Resets a static field to its default value using reflection.
    /// </summary>
    /// <param name="targetType">The type containing the static field</param>
    /// <param name="fieldName">The name of the static field to reset</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>True if the field was successfully reset, false otherwise</returns>
    public static bool ResetStaticField(Type targetType, string fieldName, ILogger? logger = null)
    {
      try
      {
        var field = targetType.GetField(fieldName,
          BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        if (field == null)
        {
          logger?.LogWarning(
            "Static field '{FieldName}' not found in type '{TypeName}'",
            fieldName, targetType.Name);
          return false;
        }

        var fieldType = field.FieldType;
        var defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;

        field.SetValue(null, defaultValue);

        logger?.LogDebug(
          "Successfully reset static field '{FieldName}' in type '{TypeName}'",
          fieldName, targetType.Name);

        return true;
      }
      catch (Exception ex)
      {
        logger?.LogError(ex,
          "Failed to reset static field '{FieldName}' in type '{TypeName}'",
          fieldName, targetType.Name);
        return false;
      }
    }

    /// <summary>
    /// Resets all static fields in a type to their default values.
    /// </summary>
    /// <typeparam name="TClass">The type containing static fields</typeparam>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>The number of fields that were successfully reset</returns>
    public static int ResetAllStaticFields<TClass>(ILogger? logger = null)
    {
      return ResetAllStaticFields(typeof(TClass), logger);
    }

    /// <summary>
    /// Resets all static fields in a type to their default values.
    /// </summary>
    /// <param name="targetType">The type containing static fields</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>The number of fields that were successfully reset</returns>
    public static int ResetAllStaticFields(Type targetType, ILogger? logger = null)
    {
      var resetCount = 0;

      var staticFields = targetType.GetFields(
        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

      foreach (var field in staticFields)
      {
        try
        {
          var fieldType = field.FieldType;
          var defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;

          field.SetValue(null, defaultValue);
          resetCount++;

          logger?.LogDebug(
            "Reset static field '{FieldName}' in type '{TypeName}'",
            field.Name, targetType.Name);
        }
        catch (Exception ex)
        {
          logger?.LogError(ex,
            "Failed to reset static field '{FieldName}' in type '{TypeName}'",
            field.Name, targetType.Name);
        }
      }

      logger?.LogDebug(
        "Reset {Count} static fields in type '{TypeName}'",
        resetCount, targetType.Name);

      return resetCount;
    }

    /// <summary>
    /// Clears a static dictionary field using reflection.
    /// </summary>
    /// <typeparam name="TClass">The type containing the static dictionary field</typeparam>
    /// <param name="fieldName">The name of the dictionary field to clear</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>True if the dictionary was successfully cleared, false otherwise</returns>
    public static bool ClearStaticDictionary<TClass>(string fieldName, ILogger? logger = null)
    {
      return ClearStaticDictionary(typeof(TClass), fieldName, logger);
    }

    /// <summary>
    /// Clears a static dictionary field using reflection.
    /// </summary>
    /// <param name="targetType">The type containing the static dictionary field</param>
    /// <param name="fieldName">The name of the dictionary field to clear</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>True if the dictionary was successfully cleared, false otherwise</returns>
    public static bool ClearStaticDictionary(Type targetType, string fieldName, ILogger? logger = null)
    {
      try
      {
        var field = targetType.GetField(fieldName,
          BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        if (field == null)
        {
          logger?.LogWarning(
            "Static dictionary field '{FieldName}' not found in type '{TypeName}'",
            fieldName, targetType.Name);
          return false;
        }

        var dictionary = field.GetValue(null);
        if (dictionary == null)
        {
          logger?.LogDebug(
            "Static dictionary field '{FieldName}' in type '{TypeName}' is already null",
            fieldName, targetType.Name);
          return true;
        }

        var clearMethod = dictionary.GetType().GetMethod("Clear");
        if (clearMethod == null)
        {
          logger?.LogWarning(
            "Clear method not found for field '{FieldName}' in type '{TypeName}'",
            fieldName, targetType.Name);
          return false;
        }

        clearMethod.Invoke(dictionary, null);

        logger?.LogDebug(
          "Successfully cleared static dictionary field '{FieldName}' in type '{TypeName}'",
          fieldName, targetType.Name);

        return true;
      }
      catch (Exception ex)
      {
        logger?.LogError(ex,
          "Failed to clear static dictionary field '{FieldName}' in type '{TypeName}'",
          fieldName, targetType.Name);
        return false;
      }
    }
  }
}