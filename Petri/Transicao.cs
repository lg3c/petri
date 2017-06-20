using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Petri
{
    /// <summary>
    /// Classe que representa uma transicao da Rede de Petri.
    /// </summary>
    internal class Transicao
    {
        internal string Nome;

        internal string TaxaProb
        {
            set
            {
                switch (value)
                {
                    case "M":
                        Distribuicao = TipoDistribuicao.Exponencial;
                        break;
                    case "P":
                        Distribuicao = TipoDistribuicao.Imediata;
                        break;
                }
            }
        }
        internal TipoDistribuicao Distribuicao;

        internal double ValorTaxaProb;

        // validacao de que todos os lugares referenciados foram devidamente descritos decorre das estruturas de dados utilizadas
        internal Dictionary<string, int> Lugares = new Dictionary<string, int>();        
        internal Dictionary<Lugar, int> LugaresAcessados = new Dictionary<Lugar, int>();

        private bool habilitada = true;
        internal bool Habilitada
        {
            get
            {
                return habilitada;
            }
            set
            {
                if (value != habilitada)
                {
                    HabilitacaoModificada = true;
                }
                else
                {
                    HabilitacaoModificada = false;
                }

                habilitada = value;
            }
        }
        internal bool HabilitacaoModificada;

        internal double TempoDisparo = 0;

        internal int Disparos = 0;

        internal enum TipoDistribuicao
        {
            Exponencial,
            Imediata
        }
    }
    
    /// <summary>
    /// Estrutura de dados garante que nenhuma transicao e descrita mais de uma vez.
    /// </summary>
    internal class ListaTransicoes : KeyedCollection<string, Transicao>
    {
        protected override string GetKeyForItem(Transicao item)
        {
            return item.Nome;
        }
    }
}
