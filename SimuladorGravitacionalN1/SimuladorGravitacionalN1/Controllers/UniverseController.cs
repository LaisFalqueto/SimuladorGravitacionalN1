using SimuladorGravitacionalN1.Models;
using SimuladorGravitacionalN1.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SimuladorGravitacionalN1.Controllers
{
    internal class UniverseController
    {
        public const double LarguraMundo = 1.0E9;
        public const double AlturaMundo = 6.0E8;

        private readonly GravadorAbstrato gravador;
        private readonly Random aleatorio = new();

        public Universo? UniversoAtual { get; private set; }

        public UniverseController(GravadorAbstrato gravador)
        {
            this.gravador = gravador ?? throw new ArgumentNullException(nameof(gravador));
        }

        public Universo CriarUniverso(int quantidade, int iteracoes, double tempoEntreIteracoes, double massaMinima, double massaMaxima)
        {
            if (massaMinima <= 0 || massaMaxima < massaMinima)
            {
                throw new ArgumentException("A massa mínima deve ser maior que zero e a massa maxima deve possuir valor valido");
            }
                List<Corpo> corpos = new();

                for(int i = 1; i <= quantidade; i++)
                {
                    double massa = massaMinima + aleatorio.NextDouble() * (massaMaxima - massaMinima);
                    double densidade = aleatorio.NextDouble() * 4000.0 + 3000.0;
                    double raio = Math.Cbrt((3.0 * (massa / densidade)) / (4.0 * Math.PI));

                    double posX = aleatorio.NextDouble() * Math.Max(1.0, LarguraMundo - 2 * raio) + raio;
                    double posY = aleatorio.NextDouble() * Math.Max(1.0, AlturaMundo - 2 * raio) + raio;

                    double velX = aleatorio.NextDouble() * 2000.0 - 1000.0;
                    double velY = aleatorio.NextDouble() * 2000.0 - 1000.0;

                    corpos.Add(new Corpo($"Corpo{i}", massa, densidade, posX, posY, velX, velY));
                }

                UniversoAtual = new Universo(corpos, iteracoes, tempoEntreIteracoes);
                IteracaoVisual = 0;
                return UniversoAtual;
            }
            public void ExecutarIteracao()
            {
            if (UniversoAtual is null)
                return;

            UniversoAtual.ExecutarIteracao(LarguraMundo, AlturaMundo);
            IteracaoVisual = UniversoAtual.IteracaoAtual;
            }

        public void Salvar(string caminho)
        {
            if(UniversoAtual is null)
            {
                throw new InvalidOperationException("Não há universo atual para salvar."); 
            }

            gravador.Salvar(UniversoAtual, caminho);
        }

        public void Carregar(string caminho)
        {
            UniversoAtual = gravador.Carregar(caminho);

            UniversoAtual.ReiniciarIteracoes();

            IteracaoVisual = 0;
        }
        public int IteracaoVisual
        {
            get; private set;
        }
        public bool Terminou()
        {
            return UniversoAtual is not null && UniversoAtual.IteracaoAtual >= UniversoAtual.QuantidadeIteracoes;
        }
    }
}
