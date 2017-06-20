using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Petri
{
    /// <summary>
    /// Classe que representa um Lugar da Rede de Petri.
    /// </summary>
    internal class Lugar
    {
        internal string Nome;
        internal int Marcas;

        // validacao de que todas as transicoes referenciadas foram devidamente descritas decorre das estruturas de dados utilizadas
        internal Dictionary<string, int> Transicoes = new Dictionary<string,int>();
        internal Dictionary<Transicao, int> TransicoesAcessadas = new Dictionary<Transicao, int>();

        internal double MarcasAcumuladas = 0;
    }

    /// <summary>
    /// Estrutura de dados garante que nenhum lugar e descrito mais de uma vez.
    /// </summary>
    internal class ListaLugares : KeyedCollection<string, Lugar>
    {
        protected override string GetKeyForItem(Lugar item)
        {
            return item.Nome;
        }
    }
}
