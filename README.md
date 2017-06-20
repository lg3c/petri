**_[college project]_**

# petri

## Introdução

O exercício trata da implementação de um simulador simples, em linha de comando, para **Redes de Petri Estocásticas Generalizadas**.
Para utilizar o simulador, abrir uma janela do prompt de comando na pasta onde se localiza o arquivo Petri.exe e executar com um comando na forma:

`Petri [-v] <descrição da rede> <número de passos>`

Onde o parâmetro `-v` produz um relatório detalhado da simulação, a descrição da rede deve ser apresentada num arquivo `.txt` no formato adequado e o número de passos deve ser um inteiro que determinará por quantos passos a simulação será executada.

## Implementação

### Valores 
Valores são simplesmente constantes numéricas nomeadas, utilizadas para aumentar a legibilidade da descrição da Rede. No simulador implementado, esses valores são armazenados numa estrutura de dados “Dictionary”.

### Lugares
Lugares da Rede de Petri, com nome e transições acessadas, e opcionalmente número inicial de marcas (default 0) e número de marcas consumidas por cada transição (default 1). No simulador implementado, cada lugar é representado por uma instância da classe Lugar, e a lista de todos os lugares é armazenada numa estrutura de dados “Keyed collection”, ou seja, coleção com chave, onde a chave é dada pelo nome do lugar.

### Transições
Transições da Rede de Petri, contendo seu nome, o tipo de distribuição (exponencial ou imediata) e sua taxa ou probabilidade, e os lugares acessados, e opcionalmente o número de marcas que cada lugar recebe quando a transição dispara (default 1). No simulador implementado, cada transição é representada por uma instância da classe Transicao, e a lista de todas as transições é armazenada numa estrutura de dados “Keyed collection”, ou seja, coleção com chave, onde a chave é dada pelo nome da transição.
