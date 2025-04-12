using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Server.Services.Auth.Models;

namespace Zarichney.Server.Services.Auth.Commands;

public record CreateApiKeyCommand : IRequest<ApiKeyResponse>
{
  [Required]
  [MaxLength(100)]
  public string Name { get; init; } = string.Empty;

  [MaxLength(255)]
  public string? Description { get; init; }

  public DateTime? ExpiresAt { get; init; }
}

public record RevokeApiKeyCommand(int ApiKeyId) : IRequest<bool>;

public record GetApiKeysQuery : IRequest<List<ApiKeyResponse>>;

public record GetApiKeyByIdQuery(int ApiKeyId) : IRequest<ApiKeyResponse?>;

public class ApiKeyResponse
{
  public int Id { get; init; }
  public string KeyValue { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public DateTime CreatedAt { get; init; }
  public DateTime? ExpiresAt { get; init; }
  public bool IsActive { get; init; }
  public string? Description { get; init; }

  public static ApiKeyResponse FromApiKey(ApiKey apiKey)
  {
    return new ApiKeyResponse
    {
      Id = apiKey.Id,
      KeyValue = apiKey.KeyValue,
      Name = apiKey.Name,
      CreatedAt = apiKey.CreatedAt,
      ExpiresAt = apiKey.ExpiresAt,
      IsActive = apiKey.IsActive,
      Description = apiKey.Description
    };
  }
}

public class CreateApiKeyCommandHandler(
    IApiKeyService apiKeyService,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreateApiKeyCommand, ApiKeyResponse>
{
  public async Task<ApiKeyResponse> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
  {
    // Get the current user ID from authenticated user
    var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
    if (string.IsNullOrEmpty(userId))
    {
      throw new UnauthorizedAccessException("User is not authenticated");
    }

    var apiKey = await apiKeyService.CreateApiKey(
        userId,
        request.Name,
        request.Description,
        request.ExpiresAt);

    return ApiKeyResponse.FromApiKey(apiKey);
  }
}

public class RevokeApiKeyCommandHandler(
      IApiKeyService apiKeyService,
      UserManager<ApplicationUser> userManager,
      IHttpContextAccessor httpContextAccessor)
       : IRequestHandler<RevokeApiKeyCommand, bool>
{

  public async Task<bool> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
  {
    // Get the current user ID from authenticated user
    var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
    if (string.IsNullOrEmpty(userId))
    {
      throw new UnauthorizedAccessException("User is not authenticated");
    }

    // First check that the API key belongs to the current user
    var apiKey = await apiKeyService.GetApiKey(request.ApiKeyId);
    if (apiKey == null || apiKey.UserId != userId)
    {
      return false;
    }

    return await apiKeyService.RevokeApiKey(request.ApiKeyId);
  }
}

public class GetApiKeysQueryHandler(
    IApiKeyService apiKeyService,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor)
      : IRequestHandler<GetApiKeysQuery, List<ApiKeyResponse>>
{

  public async Task<List<ApiKeyResponse>> Handle(GetApiKeysQuery request, CancellationToken cancellationToken)
  {
    // Get the current user ID from authenticated user
    var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
    if (string.IsNullOrEmpty(userId))
    {
      throw new UnauthorizedAccessException("User is not authenticated");
    }

    var apiKeys = await apiKeyService.GetApiKeysByUserId(userId);
    return apiKeys.Select(ApiKeyResponse.FromApiKey).ToList();
  }
}

public class GetApiKeyByIdQueryHandler(
      IApiKeyService apiKeyService,
      UserManager<ApplicationUser> userManager,
      IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetApiKeyByIdQuery, ApiKeyResponse?>
{
  public async Task<ApiKeyResponse?> Handle(GetApiKeyByIdQuery request, CancellationToken cancellationToken)
  {
    // Get the current user ID from authenticated user
    var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
    if (string.IsNullOrEmpty(userId))
    {
      throw new UnauthorizedAccessException("User is not authenticated");
    }

    var apiKey = await apiKeyService.GetApiKey(request.ApiKeyId);

    // Only return the API key if it belongs to the current user
    if (apiKey == null || apiKey.UserId != userId)
    {
      return null;
    }

    return ApiKeyResponse.FromApiKey(apiKey);
  }
}