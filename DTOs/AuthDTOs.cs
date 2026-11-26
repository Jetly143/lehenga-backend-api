namespace Rent_Lehenga_Project.DTOs;

public class RegisterDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;

    // Supported Roles: CUSTOMER, INDIVIDUAL_OWNER, SHOP_OWNER, OWNER, ADMIN, TAILOR, DELIVERY_AGENT
    public string Role { get; set; } = "CUSTOMER";

    // Optional KYC / Business fields during registration
    public string? ShopName { get; set; }
    public string? GstNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? AadharNumber { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GoogleAuthDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GoogleId { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "CUSTOMER"; // Default role if user is new
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ShopName { get; set; }
    public string? GstNumber { get; set; }
}

public class SubmitKycDto
{
    public long? UserId { get; set; }
    public string? Email { get; set; }
    public string? AadharNumber { get; set; }
    public string? AadharDocUrl { get; set; }
    public string? SelfieVideoUrl { get; set; }
    public string? ShopName { get; set; }
    public string? GstNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
}

public class VerifyEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class UserResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public string KycStatus { get; set; } = "NOT_SUBMITTED";
    public bool IsVerified { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public string? AadharNumber { get; set; }
    public string? AadharDocUrl { get; set; }
    public string? SelfieVideoUrl { get; set; }
    public string? ShopName { get; set; }
    public string? GstNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
}
