using SimuladorGravitacionalN1.Models;
using SimuladorGravitacionalN1.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SimuladorGravitacionalN1.Controllers
{
    //classe responsável por controlar o universo, incluindo a criação de corpos, execução de iterações e salvamento do estado do universo.
    internal class UniverseController
    {
        // Define a largura do espaço utilizado pela simulação, seguem o Sistema Internacional (SI).
        public const double LarguraMundo = 1.0E9;
        // Define a altura do mundo como uma constante de 6.0E8 unidades.
        public const double AlturaMundo = 6.0E8;
        //Define a constante de gravidade como 6.67430E-11, que é a constante gravitacional universal.
        private readonly GravadorAbstrato gravador;
        //Cria uma instância de Random para gerar números aleatórios.
        private readonly Random aleatorio = new();

        public Universo? UniversoAtual { get; private set; }
        //Construtor da classe UniverseController que recebe um GravadorAbstrato como parâmetro.
        public UniverseController(GravadorAbstrato gravador)
        {
            this.gravador = gravador ?? throw new ArgumentNullException(nameof(gravador));
        }
        //Método para criar um novo universo com corpos aleatórios.
        public Universo CriarUniverso(int quantidade, int iteracoes, double tempoEntreIteracoes, double massaMinima, double massaMaxima)
        {
            //Valida os parâmetros de entrada, lançando exceções se algum deles for inválido.
            if (massaMinima <= 0 || massaMaxima < massaMinima)
            {
                throw new ArgumentException("A massa mínima deve ser maior que zero e a massa maxima deve possuir valor valido");
            }

            List<Corpo> corpos = new(); // Lista que armazenará todos os corpos criados para o Universo.

            // Cria a quantidade de corpos solicitada pelo usuário.
            for (int i = 1; i <= quantidade; i++)
                {
                // Gera uma massa aleatória dentro do intervalo
                // entre a massa mínima e a massa máxima.
                double massa = massaMinima + aleatorio.NextDouble() * (massaMaxima - massaMinima);

                // Gera uma densidade aleatória entre 3000 e 7000 kg/m³.
                // A densidade é utilizada posteriormente para calcular o raio.
                double densidade = aleatorio.NextDouble() * 4000.0 + 3000.0;

                // Calcula o raio físico do corpo utilizando a relação entre massa, densidade e volume de uma esfera:
                // V = m / densidade
                // V = 4/3 * PI * r³
                //r = raiz cúbica(3m / (4PI*d)).
                double raio = Math.Cbrt((3.0 * (massa / densidade)) / (4.0 * Math.PI));

                // Gera posições aleatórias para o corpo, garantindo que ele não ultrapasse os limites do mundo.
                double posX = aleatorio.NextDouble() * Math.Max(1.0, LarguraMundo - 2 * raio) + raio;
                    double posY = aleatorio.NextDouble() * Math.Max(1.0, AlturaMundo - 2 * raio) + raio;

                //Gera velocidades aleatórias para o corpo, variando entre -1000.0 e 1000.0 em ambas as direções.
                double velX = aleatorio.NextDouble() * 2000.0 - 1000.0;
                    double velY = aleatorio.NextDouble() * 2000.0 - 1000.0;

                corpos.Add(new Corpo($"Corpo{i}", massa, densidade, posX, posY, velX, velY));
                }
            //Cria um novo universo com os corpos gerados, o número de iterações e o tempo entre iterações especificados.
            UniversoAtual = new Universo(corpos, iteracoes, tempoEntreIteracoes);
                IteracaoVisual = 0;
                return UniversoAtual;
            }
        //Método para executar uma iteração do universo atual.
        public void ExecutarIteracao()
            {
            if (UniversoAtual is null)
                return;
            //Executa a iteração atual do universo, atualizando as posições e velocidades dos corpos com base nas forças gravitacionais.
            UniversoAtual.ExecutarIteracao(LarguraMundo, AlturaMundo);
            IteracaoVisual = UniversoAtual.IteracaoAtual;
            }

        //Método para salvar o estado atual do universo em um arquivo.
        public void Salvar(string caminho)
        {
            if(UniversoAtual is null)
            {
                throw new InvalidOperationException("Não há universo atual para salvar."); 
            }
            //Salva o estado atual do universo no caminho especificado usando o gravador.
            gravador.Salvar(UniversoAtual, caminho);
        }

        //Método para carregar um universo a partir de um arquivo.
        public void Carregar(string caminho)
        {
            UniversoAtual = gravador.Carregar(caminho);

            UniversoAtual.ReiniciarIteracoes();

            IteracaoVisual = 0;
        }

        public int IteracaoVisual //Propriedade que retorna o número da iteração visual atual do universo.
        {
            get; private set;
        }
        //Método que verifica se o universo atual terminou todas as iterações.
        public bool Terminou()
        {
            return UniversoAtual is not null && UniversoAtual.IteracaoAtual >= UniversoAtual.QuantidadeIteracoes;
        }
    }
}
