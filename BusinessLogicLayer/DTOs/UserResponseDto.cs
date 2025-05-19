namespace BusinessLogicLayer.DTOs;

public record UserResponseDto(Guid UserId, string? Username, string? Email, string Gender);