using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebAPIClient.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public string CPF_CNPJ { get; set; }

        [Required]
        public int TipoCliente { get; set; }

        [Required]
        [StringLength(10)]
        public string DtNascimento { get; set; }

        public string Fone { get; set; }

        [Required]
        public string Celular { get; set; }

        [Required]
        public string Email { get; set; }

        public List<Endereco> Enderecos { get; set; }
    }
}