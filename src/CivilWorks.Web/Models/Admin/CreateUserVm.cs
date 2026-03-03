using System.ComponentModel.DataAnnotations;

namespace CivilWorks.Web.Models.Admin;

public class CreateUserVm
{
    [Required]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = "";

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(6)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = "";

    [Required]
    [Display(Name = "Perfil")]
    public string Role { get; set; } = "Funcionario";
}