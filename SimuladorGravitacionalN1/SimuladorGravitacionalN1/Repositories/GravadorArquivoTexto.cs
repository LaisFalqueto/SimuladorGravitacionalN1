using SimuladorGravitacionalN1.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimuladorGravitacionalN1.Repositories
{
    public class GravadorArquivoTexto : GravadorAbstrato
    { // Salva a configuração atual do Universo.
        public override void Salvar(
            Universo universo,
            string caminho)
        {
            // Cria o arquivo e substitui seu conteúdo caso ele já exista.
            using StreamWriter writer =
                new StreamWriter(caminho, false);

            // Primeira linha:
            // quantidade de corpos;
            // quantidade de iterações;
            // tempo entre iterações.
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

            // Depois da primeira linha, grava cada corpo.
            foreach (Corpo corpo in universo.Corpos)
            {
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

        // Carrega uma configuração salva anteriormente.
        public override Universo Carregar(
            string caminho)
        {
            if (!File.Exists(caminho))
            {
                throw new FileNotFoundException(
                    "O arquivo não foi encontrado.",
                    caminho);
            }

            // Lê todas as linhas do arquivo.
            string[] linhas =
                File.ReadAllLines(caminho);

            if (linhas.Length == 0)
            {
                throw new InvalidDataException(
                    "O arquivo está vazio.");
            }

            // Lê a primeira linha.
            string[] cabecalho =
                linhas[0].Split(';');

            if (cabecalho.Length != 3)
            {
                throw new InvalidDataException(
                    "A primeira linha do arquivo está inválida.");
            }

            // Recupera a quantidade de corpos.
            int quantidade =
                int.Parse(
                    cabecalho[0],
                    CultureInfo.InvariantCulture);

            // Recupera a quantidade de iterações.
            int iteracoes =
                int.Parse(
                    cabecalho[1],
                    CultureInfo.InvariantCulture);

            // Recupera o tempo entre as iterações.
            double tempo =
                double.Parse(
                    cabecalho[2],
                    CultureInfo.InvariantCulture);

            if (quantidade <= 0)
            {
                throw new InvalidDataException(
                    "A quantidade de corpos deve ser maior que zero.");
            }

            if (iteracoes <= 0)
            {
                throw new InvalidDataException(
                    "A quantidade de iterações deve ser maior que zero.");
            }

            if (tempo <= 0)
            {
                throw new InvalidDataException(
                    "O tempo entre iterações deve ser maior que zero.");
            }

            // Verifica se o arquivo possui todos os corpos.
            if (linhas.Length - 1 < quantidade)
            {
                throw new InvalidDataException(
                    "O arquivo não possui todos os corpos.");
            }

            List<Corpo> corpos = new();

            // Começa na linha 1 porque a linha 0 é o cabeçalho.
            for (int i = 0; i < quantidade; i++)
            {
                string[] dados =
                    linhas[i + 1].Split(';');

                // Cada corpo precisa ter 7 informações.
                if (dados.Length != 7)
                {
                    throw new InvalidDataException(
                        $"A linha do corpo {i + 1} está inválida.");
                }

                string nome = dados[0];

                double massa =
                    double.Parse(
                        dados[1],
                        CultureInfo.InvariantCulture);

                double densidade =
                    double.Parse(
                        dados[2],
                        CultureInfo.InvariantCulture);

                double posX =
                    double.Parse(
                        dados[3],
                        CultureInfo.InvariantCulture);

                double posY =
                    double.Parse(
                        dados[4],
                        CultureInfo.InvariantCulture);

                double velX =
                    double.Parse(
                        dados[5],
                        CultureInfo.InvariantCulture);

                double velY =
                    double.Parse(
                        dados[6],
                        CultureInfo.InvariantCulture);

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

            // Cria um novo Universo usando os dados
            // recuperados do arquivo.
            return new Universo(
                corpos,
                iteracoes,
                tempo);
        }
    }
}