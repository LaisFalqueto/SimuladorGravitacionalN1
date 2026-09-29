using SimuladorGravitacionalN1.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimuladorGravitacionalN1.Repositories
{
    // Classe responsável por ler e escrever o estado do Universo em arquivos de texto.
    // Herda de GravadorAbstrato, implementando os métodos Salvar e Carregar.
    public class GravadorArquivoTexto : GravadorAbstrato
    {
        // Salva a configuração atual do Universo em um arquivo texto.
        public override void Salvar(
            Universo universo,
            string caminho)
        {
            // Cria/abre o arquivo para escrita.
            // O segundo parâmetro (false) indica que o conteúdo existente será substituído.
            using StreamWriter writer =
                new StreamWriter(caminho, false);

            // Primeira linha do arquivo (cabeçalho):
            // - quantidade de corpos
            // - quantidade de iterações
            // - tempo entre iterações
            // Os valores são separados por ponto e vírgula.
            // InvariantCulture garante que o separador decimal seja ponto (.) e não vírgula,
            // evitando problemas ao abrir o arquivo em sistemas com configurações regionais diferentes.
            writer.WriteLine(
                string.Join(
                    ";",
                    universo.Corpos.Count.ToString(
                        CultureInfo.InvariantCulture),

                    universo.QuantidadeIteracoes.ToString(
                        CultureInfo.InvariantCulture),

                    universo.TempoEntreIteracoes.ToString(
                        CultureInfo.InvariantCulture)
                )
            );

            // Depois da primeira linha, grava cada corpo em uma linha separada.
            foreach (Corpo corpo in universo.Corpos)
            {
                // Cada linha contém 7 campos separados por ';':
                // Nome; Massa; Densidade; PosX; PosY; VelX; VelY
                writer.WriteLine(
                    string.Join(
                        ";",

                        corpo.Nome,

                        corpo.Massa.ToString(
                            CultureInfo.InvariantCulture),

                        corpo.Densidade.ToString(
                            CultureInfo.InvariantCulture),

                        corpo.PosX.ToString(
                            CultureInfo.InvariantCulture),

                        corpo.PosY.ToString(
                            CultureInfo.InvariantCulture),

                        corpo.VelX.ToString(
                            CultureInfo.InvariantCulture),

                        corpo.VelY.ToString(
                            CultureInfo.InvariantCulture)
                    )
                );
            }
        }

        // Carrega uma configuração salva anteriormente a partir de um arquivo texto.
        public override Universo Carregar(
            string caminho)
        {
            // Verifica se o arquivo existe antes de tentar lê-lo.
            if (!File.Exists(caminho))
            {
                throw new FileNotFoundException(
                    "O arquivo não foi encontrado.",
                    caminho);
            }

            // Lê todas as linhas do arquivo de uma vez.
            string[] linhas =
                File.ReadAllLines(caminho);

            // Se o arquivo estiver vazio, não há nada para carregar.
            if (linhas.Length == 0)
            {
                throw new InvalidDataException(
                    "O arquivo está vazio.");
            }

            // A primeira linha contém o cabeçalho com 3 valores.
            string[] cabecalho =
                linhas[0].Split(';');

            if (cabecalho.Length != 3)
            {
                throw new InvalidDataException(
                    "A primeira linha do arquivo está inválida.");
            }

            // Recupera a quantidade de corpos do cabeçalho.
            int quantidade =
                int.Parse(
                    cabecalho[0],
                    CultureInfo.InvariantCulture);

            // Recupera a quantidade de iterações.
            int iteracoes =
                int.Parse(
                    cabecalho[1],
                    CultureInfo.InvariantCulture);

            // Recupera o tempo entre iterações.
            double tempo =
                double.Parse(
                    cabecalho[2],
                    CultureInfo.InvariantCulture);

            // Valida se a quantidade de corpos é positiva.
            if (quantidade <= 0)
            {
                throw new InvalidDataException(
                    "A quantidade de corpos deve ser maior que zero.");
            }

            // Valida se a quantidade de iterações é positiva.
            if (iteracoes <= 0)
            {
                throw new InvalidDataException(
                    "A quantidade de iterações deve ser maior que zero.");
            }

            // Valida se o tempo entre iterações é positivo.
            if (tempo <= 0)
            {
                throw new InvalidDataException(
                    "O tempo entre iterações deve ser maior que zero.");
            }

            // Verifica se o arquivo possui pelo menos a quantidade de linhas
            // necessária para armazenar todos os corpos (fora o cabeçalho).
            if (linhas.Length - 1 < quantidade)
            {
                throw new InvalidDataException(
                    "O arquivo não possui todos os corpos.");
            }

            // Lista que vai armazenar os corpos lidos.
            List<Corpo> corpos = new();

            // Percorre cada linha dos corpos.
            // Começa em i + 1 porque a linha 0 é o cabeçalho.
            for (int i = 0; i < quantidade; i++)
            {
                string[] dados =
                    linhas[i + 1].Split(';');

                // Cada corpo precisa ter exatamente 7 campos.
                if (dados.Length != 7)
                {
                    throw new InvalidDataException(
                        $"A linha do corpo {i + 1} está inválida.");
                }

                // Nome do corpo (campo 0).
                string nome = dados[0];

                // Massa (campo 1).
                double massa =
                    double.Parse(
                        dados[1],
                        CultureInfo.InvariantCulture);

                // Densidade (campo 2).
                double densidade =
                    double.Parse(
                        dados[2],
                        CultureInfo.InvariantCulture);

                // Posição X (campo 3).
                double posX =
                    double.Parse(
                        dados[3],
                        CultureInfo.InvariantCulture);

                // Posição Y (campo 4).
                double posY =
                    double.Parse(
                        dados[4],
                        CultureInfo.InvariantCulture);

                // Velocidade X (campo 5).
                double velX =
                    double.Parse(
                        dados[5],
                        CultureInfo.InvariantCulture);

                // Velocidade Y (campo 6).
                double velY =
                    double.Parse(
                        dados[6],
                        CultureInfo.InvariantCulture);

                // Cria o corpo com os dados lidos e adiciona à lista.
                corpos.Add(
                    new Corpo(
                        nome,
                        massa,
                        densidade,
                        posX,
                        posY,
                        velX,
                        velY));
            }

            // Cria e retorna um novo Universo com os dados recuperados do arquivo:
            // a lista de corpos, a quantidade de iterações e o tempo entre elas.
            return new Universo(
                corpos,
                iteracoes,
                tempo);
        }
    }
}
