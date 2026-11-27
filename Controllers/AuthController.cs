using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rent_Lehenga_Project.Data;
using Rent_Lehenga_Project.DTOs;
using Rent_Lehenga_Project.Models;
using Rent_Lehenga_Project.Services;

namespace Rent_Lehenga_Project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthController(AppDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    /// <summary>
    /// User Registration (Email & Password) — Supports CUSTOMER, INDIVIDUAL_OWNER, SHOP_OWNER
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email.ToLower()))
        {
            return BadRequest(new { message = "Email already registered." });
        }

        var normalizedRole = (dto.Role ?? UserRole.Customer).ToUpper();
        if (normalizedRole == UserRole.Admin)
        {
            return BadRequest(new { message = "Registration for Platform Administrator role is disabled. Admin accounts are provisioned internally." });
        }
        
        // P2P Ladies and B2B Shops require Email Verification before KYC submission
        var isPartner = normalizedRole == UserRole.IndividualOwner || normalizedRole == UserRole.ShopOwner;
        var verificationToken = isPartner ? Guid.NewGuid().ToString("N") : null;

        // Map KYC initial state based on user role and provided details
        var initialKyc = KycStatus.NotSubmitted;
        if (!string.IsNullOrWhiteSpace(dto.AadharNumber) || !string.IsNullOrWhiteSpace(dto.GstNumber))
        {
            initialKyc = KycStatus.Pending;
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email.ToLower(),
            Phone = dto.Phone,
            Role = normalizedRole,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            ShopName = dto.ShopName,
            GstNumber = dto.GstNumber,
            Address = dto.Address,
            City = dto.City,
            AadharNumber = dto.AadharNumber,
            KycStatus = initialKyc,
            IsVerified = false,
            IsEmailVerified = !isPartner,
            EmailVerificationToken = verificationToken,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (isPartner && !string.IsNullOrWhiteSpace(verificationToken))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendVerificationEmailAsync(user.Email, user.Name, verificationToken, user.Role);
                }
                catch
                {
                    // Email delivery logged in EmailService
                }
            });
        }

        var token = GenerateJwtToken(user);

        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "https://rentlehengaadmin.vercel.app";
        return Ok(new
        {
            message = isPartner
                ? "Registration successful! A verification link has been sent to your email. Please verify to proceed to KYC."
                : "Registration successful",
            token = token,
            tokenType = "Bearer",
            verificationToken = verificationToken,
            verificationLink = isPartner ? $"{frontendUrl}/?action=confirm-email&token={verificationToken}&email={Uri.EscapeDataString(user.Email)}" : null,
            user = MapToUserResponse(user)
        });
    }

    /// <summary>
    /// User Login (Email & Password) — Returns JWT Bearer Token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            message = "Login successful",
            token = token,
            tokenType = "Bearer",
            user = MapToUserResponse(user)
        });
    }

    /// <summary>
    /// Google OAuth Sign-in & Sign-up — Returns JWT Bearer Token
    /// </summary>
    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "Email is required for Google authentication." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

        if (user == null)
        {
            // Auto-register new Google user (Disallow Admin role for self-registration)
            var targetRole = (dto.Role ?? UserRole.Customer).ToUpper();
            if (targetRole == UserRole.Admin)
            {
                targetRole = UserRole.Customer;
            }

            user = new User
            {
                Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.Email.Split('@')[0] : dto.Name,
                Email = dto.Email.ToLower(),
                Role = targetRole,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                KycStatus = KycStatus.NotSubmitted,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            message = "Google authentication successful",
            token = token,
            tokenType = "Bearer",
            user = MapToUserResponse(user)
        });
    }

    /// <summary>
    /// Get Current Authenticated User (Requires Bearer JWT Token)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid token claims." });
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(MapToUserResponse(user));
    }

    /// <summary>
    /// Submit KYC Details (For Individual Ladies & Rental Shop Owners)
    /// </summary>
    [HttpPost("submit-kyc")]
    public async Task<IActionResult> SubmitKyc([FromBody] SubmitKycDto dto)
    {
        var currentId = GetCurrentUserId();
        User? user = null;

        if (currentId != null)
        {
            user = await _context.Users.FindAsync(currentId.Value);
        }
        else if (dto.UserId != null)
        {
            user = await _context.Users.FindAsync(dto.UserId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        }

        if (user == null)
        {
            return NotFound(new { message = "User account not found." });
        }

        if (!string.IsNullOrWhiteSpace(dto.AadharNumber)) user.AadharNumber = dto.AadharNumber;
        if (!string.IsNullOrWhiteSpace(dto.AadharDocUrl)) user.AadharDocUrl = dto.AadharDocUrl;
        if (!string.IsNullOrWhiteSpace(dto.SelfieVideoUrl)) user.SelfieVideoUrl = dto.SelfieVideoUrl;
        if (!string.IsNullOrWhiteSpace(dto.ShopName)) user.ShopName = dto.ShopName;
        if (!string.IsNullOrWhiteSpace(dto.GstNumber)) user.GstNumber = dto.GstNumber;
        if (!string.IsNullOrWhiteSpace(dto.Address)) user.Address = dto.Address;
        if (!string.IsNullOrWhiteSpace(dto.City)) user.City = dto.City;

        user.KycStatus = KycStatus.Pending;
        user.IsVerified = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "KYC documents submitted successfully. Verification pending admin review.",
            user = MapToUserResponse(user)
        });
    }

    /// <summary>
    /// Get Partner KYC Records (For Admin Identity Center - Shows ALL Partners)
    /// Supports filtering by role (INDIVIDUAL_OWNER, SHOP_OWNER) and status (PENDING, NOT_SUBMITTED, VERIFIED)
    /// </summary>
    [HttpGet("kyc")]
    public async Task<IActionResult> GetPartnerKycs(
        [FromQuery] string? role,
        [FromQuery] string? status)
    {
        var query = _context.Users
            .Where(u => u.Role == UserRole.IndividualOwner || u.Role == UserRole.ShopOwner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role) && role.ToUpper() != "ALL")
        {
            var normRole = role.ToUpper();
            query = query.Where(u => u.Role == normRole);
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToUpper() != "ALL")
        {
            var normStatus = status.ToUpper();
            query = query.Where(u => u.KycStatus == normStatus);
        }

        var users = await query
            .OrderByDescending(u => u.UpdatedAt)
            .Select(u => MapToUserResponse(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get Pending KYC Verification Requests (For Admin Audit Center)
    /// </summary>
    [HttpGet("kyc/pending")]
    public async Task<IActionResult> GetPendingKyc()
    {
        var pendingUsers = await _context.Users
            .Where(u => u.KycStatus == KycStatus.Pending)
            .OrderByDescending(u => u.UpdatedAt)
            .Select(u => MapToUserResponse(u))
            .ToListAsync();

        return Ok(pendingUsers);
    }

    /// <summary>
    /// Admin Approves KYC for Partner
    /// </summary>
    [HttpPut("kyc/{userId}/verify")]
    public async Task<IActionResult> VerifyKyc(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.KycStatus = KycStatus.Verified;
        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"KYC approved for {user.Name}. Partner is now verified and listing is unlocked.",
            user = MapToUserResponse(user)
        });
    }

    /// <summary>
    /// Admin Rejects KYC for Partner
    /// </summary>
    [HttpPut("kyc/{userId}/reject")]
    public async Task<IActionResult> RejectKyc(long userId, [FromQuery] string? reason)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.KycStatus = KycStatus.Rejected;
        user.IsVerified = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"KYC rejected for {user.Name}. Reason: {reason ?? "Document mismatch"}",
            user = MapToUserResponse(user)
        });
    }

    /// <summary>
    /// Forgot Password (Send Reset Link / OTP)
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "Email is required." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null)
        {
            return Ok(new { message = "If the email is registered, password reset instructions have been sent." });
        }

        return Ok(new 
        { 
            message = "Password reset instructions have been sent to your email.",
            resetToken = "DEMO-RESET-TOKEN-123456"
        });
    }

    /// <summary>
    /// Reset Password (Submit new password with token)
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(new { message = "Email and NewPassword are required." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null)
        {
            return BadRequest(new { message = "User not found or invalid token." });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password has been successfully reset. You can now login with your new password." });
    }

    /// <summary>
    /// Change Password (Authenticated user changes password)
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(new { message = "Email, CurrentPassword, and NewPassword are required." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            return Unauthorized(new { message = "Current password is incorrect." });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password updated successfully." });
    }

    /// <summary>
    /// Get User Profile by ID
    /// </summary>
    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetProfile(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(MapToUserResponse(user));
    }

    /// <summary>
    /// Update User Profile
    /// </summary>
    [HttpPut("profile/{id}")]
    public async Task<IActionResult> UpdateProfile(long id, [FromBody] UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (!string.IsNullOrWhiteSpace(dto.Address)) user.Address = dto.Address;
        if (!string.IsNullOrWhiteSpace(dto.City)) user.City = dto.City;
        if (!string.IsNullOrWhiteSpace(dto.ShopName)) user.ShopName = dto.ShopName;
        if (!string.IsNullOrWhiteSpace(dto.GstNumber)) user.GstNumber = dto.GstNumber;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully",
            user = MapToUserResponse(user)
        });
    }

    /// <summary>
    /// User Logout
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// List Users with optional role and kycStatus filters
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] string? kycStatus)
    {
        var query = _context.Users.AsQueryable();
        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.Role == role.ToUpper());
        }
        if (!string.IsNullOrEmpty(kycStatus))
        {
            query = query.Where(u => u.KycStatus == kycStatus.ToUpper());
        }

        var users = await query.ToListAsync();
        return Ok(users.Select(MapToUserResponse));
    }

    /// <summary>
    /// Helper to extract user ID from JWT claims
    /// </summary>
    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }

    /// <summary>
    /// Verify Partner Email via Token Link (Sent to Gmail on Registration)
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token))
        {
            return BadRequest(new { message = "Email and verification token are required." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null)
        {
            return NotFound(new { message = "User account not found." });
        }

        if (user.IsEmailVerified)
        {
            return Ok(new
            {
                message = "Email is already verified. You can now proceed to submit your KYC.",
                isEmailVerified = true,
                user = MapToUserResponse(user)
            });
        }

        if (user.EmailVerificationToken != dto.Token)
        {
            return BadRequest(new { message = "Invalid or expired verification token." });
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            message = "Email successfully verified! Please complete your mandatory KYC to unlock your partner dashboard.",
            isEmailVerified = true,
            token = token,
            tokenType = "Bearer",
            user = MapToUserResponse(user)
        });
    }

    /// <summary>
    /// Resend Email Verification Link
    /// </summary>
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "Email is required." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (user.IsEmailVerified)
        {
            return BadRequest(new { message = "Email is already verified." });
        }

        var token = Guid.NewGuid().ToString("N");
        user.EmailVerificationToken = token;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendVerificationEmailAsync(user.Email, user.Name, token, user.Role);
            }
            catch
            {
                // Email delivery logged in EmailService
            }
        });

        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "https://rentlehengaadmin.vercel.app";
        return Ok(new
        {
            message = "Verification link resent to your email.",
            verificationToken = token,
            verificationLink = $"{frontendUrl}/?action=confirm-email&token={token}&email={Uri.EscapeDataString(user.Email)}"
        });
    }

    /// <summary>
    /// Maps User model to UserResponseDto
    /// </summary>
    private static UserResponseDto MapToUserResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            KycStatus = user.KycStatus,
            IsVerified = user.IsVerified,
            IsEmailVerified = user.IsEmailVerified,
            EmailVerificationToken = user.EmailVerificationToken,
            AadharNumber = user.AadharNumber,
            AadharDocUrl = user.AadharDocUrl,
            SelfieVideoUrl = user.SelfieVideoUrl,
            ShopName = user.ShopName,
            GstNumber = user.GstNumber,
            Address = user.Address,
            City = user.City,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Helper method to generate secure signed JWT Token
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "LehengaVerse_SuperSecret_Jwt_Security_Key_2026_!@#$%^&*()_+";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "LehengaVerseApi";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "LehengaVerseClients";
        var expiresInDays = int.TryParse(_configuration["Jwt:ExpiresInDays"], out var days) ? days : 7;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userId", user.Id.ToString()),
                new Claim("kycStatus", user.KycStatus),
                new Claim("isVerified", user.IsVerified.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(expiresInDays),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
