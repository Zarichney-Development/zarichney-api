using MediatR;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Zarichney.Server.Auth.Commands;

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
  public int Id { get; set; }
  public string KeyValue { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public DateTime? ExpiresAt { get; set; }
  public bool IsActive { get; set; }
  public string? Description { get; set; }

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
  private readonly IApiKeyService apiKeyService = apiKeyService ?? throw new ArgumentNullException(nameof(apiKeyService));
  private readonly UserManager<ApplicationUser> userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
  private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

  public async Task<ApiKeyResponse> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
  {
    // Get the current user ID from authenticated user
    var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
    if (string.IsNullOrEmpty(userId))
    {
      throw new UnauthorizedAccessException("User is not authenticated");
    }

    var apiKey = await apiKeyService.CreateApiKeyAsync(
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
    var apiKey = await apiKeyService.GetApiKeyByIdAsync(request.ApiKeyId);
    if (apiKey == null || apiKey.UserId != userId)
    {
      return false;
    }

    return await apiKeyService.RevokeApiKeyAsync(request.ApiKeyId);
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

    var apiKeys = await apiKeyService.GetApiKeysByUserIdAsync(userId);
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

    var apiKey = await apiKeyService.GetApiKeyByIdAsync(request.ApiKeyId);

    // Only return the API key if it belongs to the current user
    if (apiKey == null || apiKey.UserId != userId)
    {
      return null;
    }

    return ApiKeyResponse.FromApiKey(apiKey);
  }
}