using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorGravitacionalN1.Models
{
    public class Universo
    {
        // Constante gravitacional universal (m³/(kg·s²))
        private const double G = 6.67430e-11;

        // Lista interna de corpos. Só pode ser alterada dentro da classe.
        private readonly List<Corpo> corpos;

        // Número total de iterações que a simulação deve executar.
        private readonly int quantidadeIteracoes;

        // Intervalo de tempo (delta t) entre cada iteração.
        private readonly double tempoEntreIteracoes;

        // Expõe os corpos como somente leitura para quem está fora da classe.
        public IReadOnlyList<Corpo> Corpos => corpos;

        // Expõe a quantidade de iterações configurada.
        public int QuantidadeIteracoes => quantidadeIteracoes;

        // Expõe o tempo entre iterações.
        public double TempoEntreIteracoes => tempoEntreIteracoes;

        // Guarda em qual iteração a simulação está no momento.
        public int IteracaoAtual { get; private set; }

        // Construtor: recebe a lista de corpos e os parâmetros da simulação.
        public Universo(List<Corpo> corpos, int quantidadeIteracoes, double tempoEntreIteracoes)
        {
            // Valida se a lista de corpos não é nula nem vazia.
            if (corpos == null || corpos.Count == 0)
                throw new ArgumentException("O universo precisa ter pelo menos um corpo.");

            // Valida se a quantidade de iterações é positiva.
            if (quantidadeIteracoes <= 0)
                throw new ArgumentException("A quantidade de iteracoes deve ser maior que zero");

            // Valida se o tempo entre iterações é positivo.
            if (tempoEntreIteracoes <= 0)
                throw new ArgumentException("O tempo entre iteracoes deve ser maior que zero");

            this.corpos = corpos;
            this.quantidadeIteracoes = quantidadeIteracoes;
            this.tempoEntreIteracoes = tempoEntreIteracoes;
        }

        // Reinicia a contagem de iterações e limpa o estado de colisão dos corpos.
        public void ReiniciarIteracoes()
        {
            IteracaoAtual = 0;

            // Limpa qualquer flag de colisão que tenha ficado de execuções anteriores.
            foreach (Corpo corpo in corpos)
                corpo.LimparColisao();
        }

        // Executa um passo completo da simulação: gravidade, movimento, colisões e limites.
        public void ExecutarIteracao(double larguraMundo, double alturaMundo)
        {
            // Antes de tudo, limpa o estado de colisão de todos os corpos.
            // Isso evita que colisões antigas interfiram no cálculo atual.
            foreach (Corpo corpo in corpos)
            {
                corpo.LimparColisao();
            }

            // Calcula a força gravitacional entre os corpos e atualiza as velocidades.
            CalcularGravidade();

            // Move cada corpo de acordo com sua velocidade e o tempo entre iterações.
            foreach (Corpo corpo in corpos)
            {
                corpo.AtualizarPosicao(tempoEntreIteracoes);
            }

            // Verifica se houve colisões e resolve.
            TratarColisoes();

            // Corrige posições que saíram dos limites do mundo.
            CorrigirLimites(larguraMundo, alturaMundo);

            // Avança o contador de iteração.
            IteracaoAtual++;
        }

        // Calcula a aceleração gravitacional entre todos os pares de corpos.
        private void CalcularGravidade()
        {
            // Matriz para acumular as acelerações de cada corpo.
            // A primeira dimensão é o índice do corpo; a segunda é X (0) ou Y (1).
            double[,] aceleracoes = new double[corpos.Count, 2];

            // Percorre cada par de corpos apenas uma vez (i < j).
            // Assim evitamos calcular a mesma força duas vezes.
            for (int i = 0; i < corpos.Count; i++)
            {
                for (int j = i + 1; j < corpos.Count; j++)
                {
                    Corpo corpoA = corpos[i];
                    Corpo corpoB = corpos[j];

                    // Vetor que vai de A para B.
                    double dx = corpoB.PosX - corpoA.PosX;
                    double dy = corpoB.PosY - corpoA.PosY;

                    // Distância ao quadrado (evita sqrt desnecessário agora).
                    double distanciaQuadrada = dx * dx + dy * dy;

                    // Distância mínima para evitar força infinita quando estão muito próximos.
                    double distanciaMinima = corpoA.CalcularRaio() + corpoB.CalcularRaio();
                    if (distanciaQuadrada < distanciaMinima * distanciaMinima)
                    {
                        distanciaQuadrada = distanciaMinima * distanciaMinima;
                    }

                    // Distância real entre os corpos.
                    double distancia = Math.Sqrt(distanciaQuadrada);

                    // Se a distância for zero, ignora (não deveria ocorrer após o ajuste acima).
                    if (distancia == 0)
                        continue;

                    // Lei da gravitação universal: F = G * m1 * m2 / d²
                    double forcaGravitacional = G * (corpoA.Massa * corpoB.Massa) / distanciaQuadrada;

                    // Normaliza o vetor (dx, dy) para obter a direção da força.
                    double direcaoX = dx / distancia;
                    double direcaoY = dy / distancia;

                    // Componentes da força em X e Y.
                    double forcaX = forcaGravitacional * direcaoX;
                    double forcaY = forcaGravitacional * direcaoY;

                    // Aceleração = Força / Massa.
                    // A aceleração em A é na direção de B; em B é na direção oposta.
                    aceleracoes[i, 0] += forcaX / corpoA.Massa;
                    aceleracoes[i, 1] += forcaY / corpoA.Massa;
                    aceleracoes[j, 0] -= forcaX / corpoB.Massa;
                    aceleracoes[j, 1] -= forcaY / corpoB.Massa;
                }
            }

            // Aplica as acelerações acumuladas nas velocidades.
            // v = v0 + a * dt
            for (int i = 0; i < corpos.Count; i++)
            {
                Corpo corpo = corpos[i];
                corpo.DefinirVelocidade(
                    corpo.VelX + aceleracoes[i, 0] * tempoEntreIteracoes,
                    corpo.VelY + aceleracoes[i, 1] * tempoEntreIteracoes
                );
            }
        }

        // Resolve uma colisão entre dois corpos, fundindo-os em um só.
        private void ResolverColisao(Corpo c1, Corpo c2)
        {
            // A massa do novo corpo é a soma das massas.
            double novaMassa = c1.Massa + c2.Massa;

            // Conservação do momento linear para a velocidade resultante.
            double novaVelX = (
                c1.Massa * c1.VelX +
                c2.Massa * c2.VelX
            ) / novaMassa;

            double novaVelY = (
                c1.Massa * c1.VelY +
                c2.Massa * c2.VelY
            ) / novaMassa;

            // A posição resultante é o centro de massa dos dois corpos.
            double novaPosX = (
                c1.Massa * c1.PosX +
                c2.Massa * c2.PosX
            ) / novaMassa;

            double novaPosY = (
                c1.Massa * c1.PosY +
                c2.Massa * c2.PosY
            ) / novaMassa;

            // Densidade média ponderada pelas massas.
            double novaDensidade = (
                c1.Massa * c1.Densidade +
                c2.Massa * c2.Densidade
            ) / novaMassa;

            // Nome combinado para identificar a fusão.
            string novoNome = c1.Nome + " + " + c2.Nome;

            // Cria o novo corpo com os dados calculados.
            Corpo novoCorpo = new Corpo(novoNome, novaMassa, novaDensidade, novaPosX, novaPosY, novaVelX, novaVelY);

            // Remove os corpos antigos da lista.
            corpos.Remove(c1);
            corpos.Remove(c2);

            // Adiciona o novo corpo.
            corpos.Add(novoCorpo);
        }

        // Verifica e resolve todas as colisões entre os corpos.
        private void TratarColisoes()
        {
            // Flag que indica se alguma colisão ocorreu nesta passagem.
            bool ColisoesOcorreram;

            // Repete o processo enquanto houver colisões.
            // Isso é necessário porque uma fusão pode gerar um novo corpo que colida com outro.
            do
            {
                ColisoesOcorreram = false;

                // Compara todos os pares de corpos.
                for (int i = 0; i < corpos.Count; i++)
                {
                    for (int j = i + 1; j < corpos.Count; j++)
                    {
                        Corpo c1 = corpos[i];
                        Corpo c2 = corpos[j];

                        // Vetor distância entre os dois corpos.
                        double dx = c2.PosX - c1.PosX;
                        double dy = c2.PosY - c1.PosY;

                        // Distância real entre eles.
                        double distancia = Math.Sqrt(dx * dx + dy * dy);

                        // Distância mínima para que não haja sobreposição.
                        double distanciaColisao = c1.CalcularRaio() + c2.CalcularRaio();

                        // Se a distância for menor ou igual à soma dos raios, houve colisão.
                        if (distancia <= distanciaColisao)
                        {
                            // Funde os dois corpos.
                            ResolverColisao(c1, c2);

                            // Marca que houve colisão e reinicia a verificação.
                            ColisoesOcorreram = true;
                            break;
                        }
                    }

                    // Se uma colisão foi resolvida, sai do loop externo também.
                    if (ColisoesOcorreram)
                        break;
                }

            } while (ColisoesOcorreram);
        }

        // Calcula as velocidades após uma colisão elástica (atualmente não usado no código).
        // Serve para caso se queira implementar colisão com ricochete em vez de fusão.
        private static void CalcularVelocidadesAposColisao(Corpo corpoA, Corpo corpoB)
        {
            double novaVelAX = ((corpoA.Massa - corpoB.Massa) * corpoA.VelX + 2 * corpoB.Massa * corpoB.VelX) / (corpoA.Massa + corpoB.Massa);
            double novaVelBX = ((corpoB.Massa - corpoA.Massa) * corpoB.VelX + 2 * corpoA.Massa * corpoA.VelX) / (corpoA.Massa + corpoB.Massa);
            double novaVelAY = ((corpoA.Massa - corpoB.Massa) * corpoA.VelY + 2 * corpoB.Massa * corpoB.VelY) / (corpoA.Massa + corpoB.Massa);
            double novaVelBY = ((corpoB.Massa - corpoA.Massa) * corpoB.VelY + 2 * corpoA.Massa * corpoA.VelY) / (corpoA.Massa + corpoB.Massa);

            corpoA.DefinirVelocidade(novaVelAX, novaVelAY);
            corpoB.DefinirVelocidade(novaVelBX, novaVelBY);
        }

        // Separa dois corpos que estão sobrepostos (atualmente não usado no código).
        // Empurra cada um para lados opostos para evitar que fiquem presos um dentro do outro.
        private static void SepararCorpos(Corpo corpoA, Corpo corpoB, double distancia, double distanciaColisao)
        {
            // Evita divisão por zero.
            if (distancia <= 0)
                distancia = 1;

            // Calcula o quanto eles estão sobrepostos.
            double excesso = distanciaColisao - distancia;
            if (excesso <= 0)
                return;

            // Vetor normalizado da direção entre os corpos.
            double nx = (corpoB.PosX - corpoA.PosX) / distancia;
            double ny = (corpoB.PosY - corpoA.PosY) / distancia;

            // Deslocamento necessário para cada corpo (metade do excesso + uma folga).
            double deslocamento = excesso / 2.0 + 0.01;

            // Move A para trás e B para frente ao longo da normal.
            corpoA.DefinirPosicao(corpoA.PosX - nx * deslocamento, corpoA.PosY - ny * deslocamento);
            corpoB.DefinirPosicao(corpoB.PosX + nx * deslocamento, corpoB.PosY + ny * deslocamento);
        }

        // Mantém os corpos dentro dos limites do mundo e inverte a velocidade ao bater nas bordas.
        private void CorrigirLimites(double larguraMundo, double alturaMundo)
        {
            foreach (Corpo corpo in corpos)
            {
                double raio = corpo.CalcularRaio();
                double x = corpo.PosX;
                double y = corpo.PosY;
                double vx = corpo.VelX;
                double vy = corpo.VelY;

                // Borda esquerda: se o corpo saiu, reposiciona e inverte o sentido de X.
                if (x - raio < 0)
                {
                    x = raio;
                    vx = Math.Abs(vx); // Garante que a velocidade aponta para a direita.
                }
                // Borda direita: se o corpo saiu, reposiciona e inverte o sentido de X.
                else if (x + raio > larguraMundo)
                {
                    x = Math.Max(raio, larguraMundo - raio); // Evita ficar preso se o mundo for menor que o raio.
                    vx = -Math.Abs(vx); // Garante que a velocidade aponta para a esquerda.
                }

                // Borda superior: se o corpo saiu, reposiciona e inverte o sentido de Y.
                if (y - raio < 0)
                {
                    y = raio;
                    vy = Math.Abs(vy); // Garante que a velocidade aponta para baixo.
                }
                // Borda inferior: se o corpo saiu, reposiciona e inverte o sentido de Y.
                else if (y + raio > alturaMundo)
                {
                    y = Math.Max(raio, alturaMundo - raio); // Evita ficar preso se o mundo for menor que o raio.
                    vy = -Math.Abs(vy); // Garante que a velocidade aponta para cima.
                }

                // Aplica as correções de posição e velocidade.
                corpo.DefinirPosicao(x, y);
                corpo.DefinirVelocidade(vx, vy);
            }
        }
    }
}
