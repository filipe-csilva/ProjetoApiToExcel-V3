using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiToExcel.Models
{
    public class Paginacao<TDados>(TDados[] items, int itemTotal, int total)
    {
        public TDados[] Dados { get; private init; } = items;
        public int ItemsTotal { get; private init; } = total;
        public int ItemsPorPagina => Dados.Count();

        public override string ToString()
        {
            string strDados = string.Join(". ", Dados);

            return $"""
                    Dados: {strDados}
                    ItemsTotal: {ItemsTotal}
                    ItemsPorPagina: {ItemsPorPagina}
                """;
        }
    }
}
