using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Emprestimo
    {
        public int IdEmprestimo { get; set; }
        public int IsbnLivro { get; set; }
        public int IdUsuario { get; set; }
        public DateTime DataEmprestimo { get; set; }
        public DateTime DataPrevDevolucao { get; set; }
        public DateTime DataRealDevolucao { get; set; }
        public string Status { get; set; }
    }
}
