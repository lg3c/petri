using System;

namespace Petri
{
    class Petri
    {                
        /// <summary>
        /// Metodo inicial. Roda o simulador de acordo com os argumentos de chamada.
        /// </summary>
        /// <param name="args">Contem os argumentos na forma [-v] (caminho do arquivo de descricao da rede) (numero de passos)</param>
        static void Main(string[] args)
        {
            bool verbose = false;
            int i = 0;

            if (args[i] == "-v")
            {
                verbose = true;
                i++;
            }
            
            Rede rede = new Rede();
            if (rede.lerDescricaoDaRede(args[i]))
            {
                i++;
                rede.simular(int.Parse(args[i]), verbose);                
            }
            else
            {
                Console.WriteLine("Erro!");
            }
        }        
    }
}
