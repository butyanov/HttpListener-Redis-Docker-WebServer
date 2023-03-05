using System.ComponentModel.DataAnnotations;

namespace WebServer.dto;

public class UserLoginDto
{
    [Required(ErrorMessage = "Email field has must specified.")]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password field has to be specified.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password field length should be between 8 and 100 literals.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
}