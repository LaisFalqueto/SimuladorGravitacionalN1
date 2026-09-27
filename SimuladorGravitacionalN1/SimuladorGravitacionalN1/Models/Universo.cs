using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorGravitacionalN1.Models
{
    public class Universo
    {
        private const double G = 6.67430e-11; // Constante gravitacional em m³/(kg·s²)

        private readonly List<Corpo> corpos;
        private readonly int quantidadeIteracoes;
        private readonly double tempoEntreIteracoes;

        public IReadOnlyList<Corpo> Corpos => corpos;
        public int QuantidadeIteracoes => quantidadeIteracoes;
        public double TempoEntreIteracoes => tempoEntreIteracoes;
        public int IteracaoAtual { get; private set; }

        public Universo(List<Corpo> corpos, int quantidadeIteracoes, double tempoEntreIteracoes)
        {
            if (corpos == null || corpos.Count == 0)
                throw new ArgumentException("O universo precisa ter pelo menos um corpo.");
            if (quantidadeIteracoes <= 0)
                throw new ArgumentException("A quantidade de iteracoes deve ser maior que zero");
            if(tempoEntreIteracoes<=0)
                throw new ArgumentException("O tempo entre iteracoes deve ser maior que zero");

            this.corpos = corpos;
            this.quantidadeIteracoes = quantidadeIteracoes;
            this.tempoEntreIteracoes = tempoEntreIteracoes;
        }

        public void ReiniciarIteracoes()
        {
            IteracaoAtual = 0;
            foreach (Corpo corpo in corpos)
                corpo.LimparColisao();
        }

        public void ExecutarIteracao(double larguraMundo, double alturaMundo)
        {
            foreach (Corpo corpo in corpos)
            {
                corpo.LimparColisao();
            }

            CalcularGravidade();

            foreach (Corpo corpo in corpos)
            {
                corpo.AtualizarPosicao(tempoEntreIteracoes);
            }
            TratarColisoes();
            CorrigirLimites(larguraMundo, alturaMundo);

            IteracaoAtual++;
        }
        private void CalcularGravidade()
        {
            double[,] aceleracoes = new double[corpos.Count, 2];

            //percorre cada par de corpos apenas uma vez para calcular a força gravitacional entre eles
            for(int i = 0; i < corpos.Count; i++)
            {
                for (int j = i + 1; j < corpos.Count; j++)
                {
                    Corpo corpoA = corpos[i];
                    Corpo corpoB = corpos[j];

                    double dx = corpoB.PosX - corpoA.PosX;
                    double dy = corpoB.PosY - corpoA.PosY;

                    double distanciaQuadrada = dx * dx + dy * dy;

                    double distanciaMinima = corpoA.CalcularRaio() + corpoB.CalcularRaio();
                    if (distanciaQuadrada < distanciaMinima * distanciaMinima)
                    { distanciaQuadrada = distanciaMinima * distanciaMinima; }

                    double distancia = Math.Sqrt(distanciaQuadrada);
                    if (distancia == 0)
                        continue;

                    double forcaGravitacional = G * (corpoA.Massa * corpoB.Massa) / distanciaQuadrada;

                    double direcaoX = dx / distancia;
                    double direcaoY = dy / distancia;

                    double forcaX = forcaGravitacional * direcaoX;
                    double forcaY = forcaGravitacional * direcaoY;

                    aceleracoes[i, 0] += forcaX / corpoA.Massa;
                    aceleracoes[i, 1] += forcaY / corpoA.Massa;
                    aceleracoes[j, 0] -= forcaX / corpoB.Massa;
                    aceleracoes[j, 1] -= forcaY / corpoB.Massa;
                }
            }

            for(int i = 0; i <corpos.Count; i++)
            {
                Corpo corpo = corpos[i];
                corpo.DefinirVelocidade(
                    corpo.VelX + aceleracoes[i, 0] * tempoEntreIteracoes,
                    corpo.VelY + aceleracoes[i, 1] * tempoEntreIteracoes
                    );
            }
        }

        private void ResolverColisao(Corpo c1, Corpo c2)
        {
            double novaMassa = c1.Massa + c2.Massa;

            double novaVelX =(
                c1.Massa * c1.VelX+
                c2.Massa * c2.VelX
                ) / novaMassa;

            double novaVelY =(
                c1.Massa * c1.VelY+
                c2.Massa * c2.VelY
                ) / novaMassa;

            double novaPosX = (
                c1.Massa * c1.PosX +
                c2.Massa * c2.PosX
                ) / novaMassa;

            double novaPosY = (
                c1.Massa * c1.PosY +
                c2.Massa * c2.PosY
                ) / novaMassa;

            double novaDensidade = (
                c1.Massa * c1.Densidade +
                c2.Massa * c2.Densidade
                ) / novaMassa;

            string novoNome = c1.Nome + " + " + c2.Nome; 
            Corpo novoCorpo = new Corpo(novoNome, novaMassa, novaDensidade, novaPosX, novaPosY, novaVelX, novaVelY);

            corpos.Remove(c1);
            corpos.Remove(c2);

            corpos.Add(novoCorpo);
        }

        private void TratarColisoes()
        {
            bool ColisoesOcorreram;

            do
            {
                ColisoesOcorreram = false;
                
                for (int i = 0; i < corpos.Count; i++)
                {
                    for (int j = i + 1; j <corpos.Count; j++)
                    {
                        Corpo c1 = corpos[i];
                        Corpo c2 = corpos[j];

                        double dx = c2.PosX - c1.PosX;
                        double dy = c2.PosY - c1.PosY;

                        double distancia = Math.Sqrt(dx * dx + dy * dy);

                        double distanciaColisao = c1.CalcularRaio() + c2.CalcularRaio();

                        if (distancia <= distanciaColisao)
                        {
                            ResolverColisao(c1, c2);
                            ColisoesOcorreram = true;
                            break;
                        }
                    }
                    if (ColisoesOcorreram)
                        break;
                }

            } while (ColisoesOcorreram);
        }

        private static void CalcularVelocidadesAposColisao(Corpo corpoA, Corpo corpoB)
        {
            double novaVelAX = ((corpoA.Massa - corpoB.Massa) * corpoA.VelX + 2 * corpoB.Massa * corpoB.VelX) / (corpoA.Massa + corpoB.Massa);
            double novaVelBX = ((corpoB.Massa - corpoA.Massa) * corpoB.VelX + 2 * corpoA.Massa * corpoA.VelX) / (corpoA.Massa + corpoB.Massa);
            double novaVelAY = ((corpoA.Massa - corpoB.Massa) * corpoA.VelY + 2 * corpoB.Massa * corpoB.VelY) / (corpoA.Massa + corpoB.Massa);
            double novaVelBY = ((corpoB.Massa - corpoA.Massa) * corpoB.VelY + 2 * corpoA.Massa * corpoA.VelY) / (corpoA.Massa + corpoB.Massa);

            corpoA.DefinirVelocidade(novaVelAX, novaVelAY);
            corpoB.DefinirVelocidade(novaVelBX, novaVelBY);
        }

        private static void SepararCorpos(Corpo corpoA, Corpo corpoB, double distancia, double distanciaColisao)
        {
            if (distancia <= 0)
                distancia = 1;

            double excesso = distanciaColisao - distancia;
            if (excesso <= 0)
                return;

            double nx = (corpoB.PosX - corpoA.PosX) / distancia;
            double ny = (corpoB.PosY - corpoA.PosY) / distancia;
            double deslocamento = excesso / 2.0 + 0.01;

            corpoA.DefinirPosicao(corpoA.PosX - nx * deslocamento, corpoA.PosY - ny * deslocamento);
            corpoB.DefinirPosicao(corpoB.PosX + nx * deslocamento, corpoB.PosY + ny * deslocamento);
        }

        private void CorrigirLimites(double larguraMundo, double alturaMundo)
        {
            foreach (Corpo corpo in corpos){
                double raio = corpo.CalcularRaio();
                double x = corpo.PosX;
                double y = corpo.PosY;
                double vx = corpo.VelX;
                double vy = corpo.VelY;

                if (x - raio < 0)
                {
                    x = raio;
                    vx = Math.Abs(vx);
                }
                else if(x + raio > larguraMundo)
                {
                    x = Math.Max(raio, larguraMundo - raio);
                    vx = -Math.Abs(vx);
                }

                if (y - raio < 0)
                {
                    y = raio;
                    vy = Math.Abs(vy);
                }
                else if (y + raio > alturaMundo)
                {
                    y = Math.Max(raio, alturaMundo - raio);
                    vy = -Math.Abs(vy);
                }

                corpo.DefinirPosicao(x, y);
                corpo.DefinirVelocidade(vx, vy);
            }
        }
    }
}
