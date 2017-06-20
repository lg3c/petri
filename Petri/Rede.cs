using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Petri
{
    /// <summary>
    /// Classe que representa a Rede de Petri a ser simulada.
    /// </summary>
    internal class Rede
    {
        // validacao de que todas as definicoes de valores foram devidamente descritas decorre da estrutura de dados utilizada
        internal Dictionary<string, double> Valores = new Dictionary<string, double>(); // definicoes de valores
        
        internal ListaLugares Lugares = new ListaLugares(); // definicoes de lugares
        internal ListaTransicoes Transicoes = new ListaTransicoes(); // definicoes de transicoes

        double Relogio = 0; // relogio que conta o tempo virtual de simulacao

        /// <summary>
        /// Le a descricao passada como parametro para o programa e monta a Rede de Petri a ser simulada.
        /// </summary>
        /// <param name="fullPath">Caminho do arquivo contendo a descricao da Rede de Petri.</param>
        /// <returns>Verdadeiro se leitura da descricao deu certo, falso caso contrario.</returns>
        internal bool lerDescricaoDaRede(string fullPath)
        {
            bool success;
            string line;                        

            try
            {                
                StreamReader file = new StreamReader(fullPath);

                while ((line = file.ReadLine()) != null)
                {
                    line = line.Replace(":", " : ");
                    Queue<string> definition = new Queue<string>(line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries));

                    if (!definition.Any())
                    {
                        continue;
                    }

                    switch (definition.Dequeue())
                    {
                        case "D": // caso de definicao de valor
                            Valores.Add(definition.Dequeue(), double.Parse(definition.Dequeue(), CultureInfo.InvariantCulture));
                            break;

                        case "L": // caso de definicao de lugar
                            Lugar lugar = new Lugar() { Nome = definition.Dequeue() };

                            if (definition.Count > 0)
                            {
                                if (definition.Peek() == ":")
                                {
                                    definition.Dequeue();
                                    lugar.Marcas = intParameterFromNumberOrValue(definition.Dequeue());
                                }

                                while (definition.Count > 0)
                                {
                                    string nomeTransicao = definition.Dequeue();
                                    int peso = 1;
                                    if (definition.Count > 0
                                        && definition.Peek() == ":")
                                    {
                                        definition.Dequeue();
                                        peso = int.Parse(definition.Dequeue());
                                    }
                                    lugar.Transicoes.Add(nomeTransicao, peso);
                                }
                            }
                            Lugares.Add(lugar);
                            break;

                        case "T": // caso de definicao de transicao
                            Transicao transicao = new Transicao()
                            {
                                Nome = definition.Dequeue(),
                                TaxaProb = definition.Dequeue(),
                                ValorTaxaProb = doubleParameterFromNumberOrValue(definition.Dequeue())
                            };

                            while (definition.Count > 0)
                            {
                                string nomeLugar = definition.Dequeue();
                                int peso = 1;
                                if (definition.Count > 0
                                    && definition.Peek() == ":")
                                {
                                    definition.Dequeue();
                                    peso = int.Parse(definition.Dequeue());
                                }
                                transicao.Lugares.Add(nomeLugar, peso);
                            }
                            Transicoes.Add(transicao);
                            break;
                        case "//": // linha de comentario na descricao ignorada
                        case "--": // linha de comentario na descricao ignorada
                            break;
                        default:
                            throw new Exception("Sintaxe incorreta na descricao da rede de Petri.");
                    }
                }
                file.Close();

                criarReferencias();

                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Cria referencias entre objetos que representam lugares e objetos que representam transicoes acessadas.
        /// Cria referencias entre objetos que representam transicoes e objetos que representam lugares acessados.
        /// </summary>
        void criarReferencias()
        {
            foreach (Lugar lugar in Lugares)
            {
                foreach (var transicao in lugar.Transicoes)
                {
                    lugar.TransicoesAcessadas.Add(Transicoes[transicao.Key], transicao.Value);
                }
            }

            foreach (Transicao transicao in Transicoes)
            {
                foreach (var lugar in transicao.Lugares)
                {
                    transicao.LugaresAcessados.Add(Lugares[lugar.Key], lugar.Value);
                }
            }
        }       

        /// <summary>
        /// Metodo auxiliar para leitura de numeros/valores representando inteiros da descricao da Rede de Petri.
        /// </summary>        
        int intParameterFromNumberOrValue(string numOrValue)
        {
            int num;
            if (!int.TryParse(numOrValue, out num))
            {
                double valor;
                if (Valores.TryGetValue(numOrValue, out valor))
                {
                    num = (int)valor;
                }
            }
            return num;
        }

        /// <summary>
        /// Metodo auxiliar para leitura de numeros/valores representando numeros em ponto flutuante da descricao da Rede de Petri.
        /// </summary>        
        double doubleParameterFromNumberOrValue(string numOrValue)
        {
            double num;
            if (!double.TryParse(numOrValue, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
            {
                Valores.TryGetValue(numOrValue, out num);
            }
            return num;
        }

        /// <summary>
        /// Metodo que executa a simulacao da Rede de Petri.
        /// </summary>
        /// <param name="passos">Numero de passos a serem executados na simulacao.</param>
        /// <param name="verbose">Determina se detalhes passo a passo da execucao devem ser exibidos ou nao.</param>
        internal void simular(int passos, bool verbose)
        {
            Random random = new Random();
            int i = 0;

            atualizarTransacoesHabilitadas(false);

            while (i < passos)
            {
                Transicao dispara = new Transicao();
                double aleatoria;                                

                var transicoesHabilitadas = Transicoes.Where(t => t.Habilitada);                
                
                var transicoesHabilitadasP = transicoesHabilitadas.Where(t => t.Distribuicao == Transicao.TipoDistribuicao.Imediata);
                var transicoesHabilitadasM = transicoesHabilitadas.Where(t => t.Distribuicao == Transicao.TipoDistribuicao.Exponencial);

                // verifica se ha transicoes imediatas habilitadas para dispara-las prioritariamente em relacao as temporizadas
                if (transicoesHabilitadasP.Any())
                {
                    // se houver apenas uma, ela deve disparar
                    if (transicoesHabilitadasP.Count() == 1)
                    {
                        dispara = transicoesHabilitadasP.First();
                    }
                    else // se houver mais de uma, devera ser decidido qual disparar
                    {
                        if (transicoesHabilitadasP.Sum(t => t.ValorTaxaProb) != 1f)
                        {
                            Console.WriteLine("Erro! Probabilidade total das transicoes imediatas difere de 1.");
                            return;
                        }

                        aleatoria = random.NextDouble();
                        double distribuicaoAcumulada = 0;

                        // determina a partir de variavel aleatoria uniforme de acordo com a 
                        // distribuicao de probabilidades especificada
                        // qual das transicoes habilitadas deve ser disparada
                        foreach (Transicao transicao in transicoesHabilitadasP)
                        {
                            if ((aleatoria >= distribuicaoAcumulada)
                                && (aleatoria < distribuicaoAcumulada + transicao.ValorTaxaProb))
                            {
                                dispara = transicao;
                            }
                            distribuicaoAcumulada += transicao.ValorTaxaProb;
                        } 
                    }
                }
                else if (transicoesHabilitadasM.Any()) // em caso de transicoes temporizadas
                {
                    foreach (Transicao transicao in transicoesHabilitadasM)
                    {
                        // transformando em distribuicao exponencial pelo metodo da funcao inversa
                        aleatoria = random.NextDouble();
                        transicao.TempoDisparo = Math.Log(1 - aleatoria) / (-transicao.ValorTaxaProb);
                    }

                    // dispara a transicao habilitada com menor tempo de disparo
                    dispara = transicoesHabilitadasM.OrderBy(t => t.TempoDisparo).First();

                    // atualiza numero que sera utilizado para determinar numero medio de marcas ao fim da simulacao
                    foreach (Lugar lugar in Lugares.Where(l => l.Marcas > 0))
                    {
                        lugar.MarcasAcumuladas += lugar.Marcas * dispara.TempoDisparo;
                    }                    
                }

                // atualiza numero que sera utilizado para determinar taxa media de disparo ao fim da simulacao
                dispara.Disparos++;

                if (verbose)
                {
                    Console.WriteLine(string.Format("----- PASSO {0} -----", i));
                    
                    Console.Write("Transicoes habilitadas: ");
                    foreach (Transicao transicao in transicoesHabilitadas)
                    {
                        Console.Write(string.Format("{0}: {1} s. ", transicao.Nome, transicao.TempoDisparo));
                    }
                    Console.WriteLine();

                    Console.WriteLine(string.Format("Transicao que vai disparar: {0}", dispara.Nome));
                }

                // decrementa marcas dos lugares que acessam a transicao disparada
                foreach (Lugar lugar in Lugares.Where(l => l.TransicoesAcessadas.Keys.Contains(dispara)))
                {
                    lugar.Marcas -= lugar.TransicoesAcessadas[dispara];

                    if (verbose)
                    {
                        Console.WriteLine(string.Format("* lugar decrementado: {0} [-{1}].",
                            lugar.Nome,
                            lugar.TransicoesAcessadas[dispara]));
                    }
                }

                // incrementa marcas nos lugares acessados pela transicao disparada
                foreach (Lugar lugar in dispara.LugaresAcessados.Keys)
                {
                    lugar.Marcas += dispara.LugaresAcessados[lugar];

                    if (verbose)
                    {
                        Console.WriteLine(string.Format("* lugar incrementado:: {0} [+{1}].",
                            lugar.Nome,
                            dispara.LugaresAcessados[lugar]));
                    }
                }                

                // incrementa tempo virtual de simulacao (em 0 para transicoes imediatas)
                Relogio += dispara.TempoDisparo;

                atualizarTransacoesHabilitadas(verbose);

                if (verbose)
                {
                    Console.WriteLine(); 
                }

                i++;
            }

            Console.WriteLine(string.Format("Tempo (virtual) total de simulacao: {0} s.", Relogio));
            
            foreach (Lugar lugar in Lugares)
            {
                Console.WriteLine(string.Format("Numero medio de marcas no lugar {0}: {1}", lugar.Nome, lugar.MarcasAcumuladas / Relogio));
            }

            Console.WriteLine(string.Format("Numero medio de marcas na rede: {0}", Lugares.Sum(l => l.MarcasAcumuladas) / Relogio));

            foreach (Transicao transicao in Transicoes)
            {
                Console.WriteLine(string.Format("Taxa media de disparo da transicao {0}: {1}", transicao.Nome, transicao.Disparos / Relogio));
            }
        }

        /// <summary>
        /// Atualiza quais transicoes estao habilitadas/desabilitadas.
        /// </summary>
        /// <param name="verbose">Determina se detalhes das transicoes que mudaram de status devem ser exibidos ou nao.</param>
        internal void atualizarTransacoesHabilitadas(bool verbose)
        {                                    
            foreach (Transicao transicao in Transicoes)
            {
                bool habilitada = true;

                var lugaresQueAcessam = Lugares.Where(l => l.TransicoesAcessadas.Keys.Contains(transicao));
                
                foreach (Lugar lugar in lugaresQueAcessam)
                {
                    if (lugar.Marcas < lugar.TransicoesAcessadas[transicao])
                    {
                        habilitada = false;                                           
                    }                    
                }

                transicao.Habilitada = habilitada;             
            }

            if (verbose)
            {
                foreach (Transicao transicao in Transicoes.Where(t => t.HabilitacaoModificada))
                {
                    Console.WriteLine(string.Format("* transicao {0}: {1}",
                        transicao.Habilitada ? "provisoriamente habilitada" : "desabilitada",
                        transicao.Nome));
                }
            }
        }
    }
}
