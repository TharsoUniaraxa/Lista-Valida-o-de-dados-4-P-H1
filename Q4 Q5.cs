using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{

    public class RaAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                return new ValidationResult("RA é obrigatório.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(s, @"^RA\d{6}$"))
                return new ValidationResult("RA inválido. Formato esperado: 'RA' seguido de 6 dígitos (ex: RA123456).");

            return ValidationResult.Success;
        }
    }

    public class CodigoProdutoAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                return new ValidationResult("codigoProduto é obrigatório.");

            if (s.Length != 8)
                return new ValidationResult("codigoProduto deve ter exatamente 8 caracteres.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(s, @"^[A-Z]{3}-\d{4}$"))
                return new ValidationResult("codigoProduto inválido. Formato esperado: 'AAA-1234' (3 letras maiúsculas, hífen, 4 números).");

            return ValidationResult.Success;
        }
    }

    public class CpfAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                return new ValidationResult("CPF é obrigatório.");

            var digits = new string(s.Where(char.IsDigit).ToArray());
            if (digits.Length != 11)
                return new ValidationResult("CPF inválido. Deve conter 11 dígitos.");

            if (Enumerable.Range(0, 10).Any(d => digits == new string(char.Parse(d.ToString()), 11)))
                return new ValidationResult("CPF inválido.");

            bool ValidateCheckDigit(string num, int[] mult)
            {
                var sum = 0;
                for (int i = 0; i < mult.Length; i++)
                    sum += (num[i] - '0') * mult[i];

                var rest = sum % 11;
                var dig = rest < 2 ? 0 : 11 - rest;
                return dig == (num[mult.Length] - '0');
            }

            var firstOk = ValidateCheckDigit(digits, new int[] { 10,9,8,7,6,5,4,3,2 });
            var secondOk = ValidateCheckDigit(digits, new int[] { 11,10,9,8,7,6,5,4,3,2 });

            if (!firstOk || !secondOk)
                return new ValidationResult("CPF inválido.");

            return ValidationResult.Success;
        }
    }

    public class Aluno
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [Ra]
        public string Ra { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Cpf]
        public string Cpf { get; set; }

        public bool Ativo { get; set; } = true;
    }

    public class Produto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(200)]
        public string Descricao { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero.")]
        public decimal Preco { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Estoque não pode ser negativo.")]
        public int Estoque { get; set; }

        [Required]
        [CodigoProduto]
        public string CodigoProduto { get; set; }
    }


    [ApiController]
    [Route("api/[controller]")]
    public class AlunoController : ControllerBase
    {
        [HttpPost]
        public IActionResult Create([FromBody] Aluno aluno)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(new { message = "Aluno válido", data = aluno });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        [HttpPost]
        public IActionResult Create([FromBody] Produto produto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(new { message = "Produto válido", data = produto });
        }
    }
}
