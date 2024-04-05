using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class AuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender; // E-posta göndermek için servis



    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;

    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized("Kullanıcı bulunamadı.");
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var token = GenerateJwtToken(user); // user objesi ile token oluştur
           
            return Ok(new { Token = token });
        }
        else if (result.IsNotAllowed)
        {
            // Burada kullanıcıya özel durumu açıklayabilirsiniz, örneğin e-posta doğrulama gerekiyor
            return Unauthorized("Giriş izni yok. Lütfen hesabınızı doğrulayın.");
        }
        else if (result.IsLockedOut)
        {
            return Unauthorized("Hesap kilitlendi.");
        }
        else
        {
            return Unauthorized("Giriş başarısız.");
        }
    }


    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Name, Surname = model.Surname };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Token'ı URL'e eklemek için UrlEncoder kullanın
            var encodedCode = System.Net.WebUtility.UrlEncode(code);
            var callbackUrl = Url.Action("ConfirmEmail", "Auth",
                new { userId = user.Id, code = encodedCode }, protocol: HttpContext.Request.Scheme);

            await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");

            return Ok("User registered successfully and verification email sent.");
        }

        return BadRequest(result.Errors);
    }
    [HttpGet]
    [Route("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        // Token'ı çözümle
        code = System.Net.WebUtility.UrlDecode(code);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded)
        {
            return View("ConfirmEmailSuccess");
        }
        else
        {
            return View("Error");
        }
    }
    [HttpDelete("delete-user")]
    public async Task<ActionResult> DeleteUser([FromBody] UserEmailModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return Ok("Kullanıcı başarıyla silindi.");
        }
        else
        {
            return BadRequest("Kullanıcı silinirken bir hata oluştu.");
        }
    }

    [HttpPut]
    [Route("update-user")]
    public async Task<IActionResult> UpdateUser([FromBody] userUpdateModel model)
    {
       

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Kullanıcı bilgilerini güncelle
        user.Name = model.Name;
        user.Surname = model.Surname;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.bio = model.bio;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "User updated successfully." });
    }



    [EnableCors("AllowAll")]
    [HttpGet]
    [Route("userinfo")]
    public IActionResult GetUserInfo()
    {
        // HttpContext.User özelliğinden kullanıcı claim'lerini al
        var nameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var surnameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (nameClaim == null || surnameClaim == null || emailClaim == null)
        {
            return NotFound("Kullanıcı bilgileri bulunamadı.");
        }

        // Kullanıcı bilgilerini içeren bir obje oluştur ve geri dön
        var userInfo = new
        {
            Name = nameClaim,
            Surname = surnameClaim,
            Email = emailClaim
        };

        return Ok(userInfo);
    }


    private string GenerateJwtToken(ApplicationUser user) // ApplicationUser, kullanıcı veritabanı modelinizi temsil etmelidir.
    {
        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.Name ?? ""), // Kullanıcı adı claim'i
        new Claim(ClaimTypes.Surname, user.Surname ?? ""), // Soyadı claim'i
        new Claim(ClaimTypes.Email, user.Email),  // Kullanıcının e-posta adresi
        // Diğer gerekli claimler
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("44b4a9ee46e73d9c74122f2d1916b40bbd9ce3296ed6db38249821f39bb2853a"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(10), // Token'ın geçerlilik süresi
            Issuer = "serhat",
            Audience = "myblogapp",
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    [HttpPost]
    [Route("password-reset-request")]
    public async Task<IActionResult> PasswordResetRequest([FromBody] PasswordResetModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Kullanıcı bulunamazsa, güvenlik nedeniyle genel bir mesaj dön
            return Ok("Şifre sıfırlama talimatları, eğer e-posta sistemimizde kayıtlıysa, gönderilmiştir.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Token'ı e-posta ile gönderme işlemini burada gerçekleştirin
        // Örneğin: SendPasswordResetEmailAsync(model.Email, user, token);

        return Ok("Şifre sıfırlama talimatları e-posta adresinize gönderildi.");
    }
    [HttpPost]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Kullanıcı bulunamazsa, güvenlik nedeniyle genel bir mesaj dön
            return BadRequest("Şifre sıfırlama işlemi başarısız.");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded)
        {
            return Ok("Şifreniz başarıyla sıfırlandı.");
        }

        // Hata mesajlarını döndür
        return BadRequest(result.Errors);
    }

    [HttpPost]
    [Route("deleteaccount")]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync(); // Oturumu sonlandır
            return Ok("Hesap başarıyla silindi.");
        }

        return BadRequest(result.Errors);
    }



}
public class PasswordResetModel
{
    [Required]
    public required string Email { get; set; }
}

public class ResetPasswordModel
{
    [Required]
    public string Token { get; set; }

    [Required]
    public string NewPassword { get; set; }
    [Required]
    public string Email { get; set; }
}

public class UserLoginModel
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class RegisterModel
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Surname { get; set; }
}
public class userUpdateModel
{
    [Required]
    public string UserId { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Surname { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string bio { get; set; }
}
public class UserEmailModel
{
    public string Email { get; set; }
}