using System.ComponentModel.DataAnnotations;

namespace WebServer.models;

public class UpdateUserData
{
    [Required(ErrorMessage = "Id field has must specified.")]
    public Guid Id { get; set; }
    
    [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed.")]
    [DataType(DataType.EmailAddress)]
    [EmailAddress]
    public string Email { get; set; }
    
    [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Nickname field length should be between 3 and 100 literals.")]
    public string NickName { get; set; }
    
    [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed.    ")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password field length should be between 8 and 100 literals.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}