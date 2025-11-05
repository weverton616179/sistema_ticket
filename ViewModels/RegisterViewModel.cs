using System.ComponentModel.DataAnnotations;

namespace TicketSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(60, ErrorMessage = "Nome pode ter no máximo 60 caracteres.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sobrenome é obrigatório.")]
        [MaxLength(80, ErrorMessage = "Sobrenome pode ter no máximo 80 caracteres.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmação de senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "As senhas não conferem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
