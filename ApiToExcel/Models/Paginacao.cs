using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiToExcel.Models
{
    public class Paginacao<TDados>
    {
        public Paginacao(TDados[] dados, int itemTotal)
        {
            Dados = dados;
            ItemTotal = itemTotal;
        }

        public TDados[] Dados { get; private init; }
        public int ItemTotal { get; private init; }
        public int ItensPorPagina => Dados.Count();
    }
}
