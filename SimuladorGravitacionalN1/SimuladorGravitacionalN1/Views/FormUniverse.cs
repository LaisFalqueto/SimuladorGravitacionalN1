using System.Drawing.Drawing2D;
using SimuladorGravitacionalN1.Controllers;
using SimuladorGravitacionalN1.Models;

namespace SimuladorGravitacionalN1.Views
{
    // Formulário principal da aplicação, responsável por exibir o universo
    // e os controles de simulação. Herda de Form (Windows Forms).
    internal class FormUniverse : Form
    {
        // Referência ao controller que gerencia a lógica do universo.
        private readonly UniverseController controller;

        // Controles da interface.
        private Panel painelUniverso = null!;          // Área onde os corpos são desenhados.
        private Panel painelControles = null!;         // Painel flutuante com botões e inputs.
        private Button btnNovo = null!;
        private Button btnIniciar = null!;
        private Button btnParar = null!;
        private Button btnSalvarEstado = null!;
        private Button btnCarregarEstado = null!;
        private NumericUpDown nudCorpos = null!;
        private NumericUpDown nudIteracoes = null!;
        private NumericUpDown nudTempo = null!;
        private NumericUpDown nudMassaMinima = null!;
        private NumericUpDown nudMassaMaxima = null!;
        private Label lblStatus = null!;
        private Label lblColisao = null!;
        private System.Windows.Forms.Timer timer = null!; // Timer para animar a simulação.

        // Caminho do arquivo onde o estado inicial é salvo automaticamente.
        private readonly string arquivoInicial;

        // Recursos GDI+ reutilizáveis (criados UMA vez, liberados no FormClosed)
        // Isso evita alocações repetidas durante o desenho, melhorando a performance.
        private readonly Font fonteCorpo = new Font("Segoe UI", 8, FontStyle.Bold);
        private readonly Brush pincelFundo = new SolidBrush(Color.White);
        private readonly Brush pincelColidiu = new SolidBrush(Color.Red);
        private readonly Brush pincelMassivo = new SolidBrush(Color.DarkGreen);
        private readonly Brush pincelNormal = new SolidBrush(Color.LightSkyBlue);
        private readonly Pen canetaContorno = new Pen(Color.Black, 1f);
        private readonly Pen canetaGrade = new Pen(Color.FromArgb(220, 220, 220), 1);

        // Construtor: recebe o controller e monta a interface.
        public FormUniverse(UniverseController controller)
        {
            this.controller = controller;
            arquivoInicial = Path.Combine(AppContext.BaseDirectory, "posicao_inicial.txt");

            // Configurações da janela.
            Text = "Simulador Gravitacional 2D";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1200, 760);
            ClientSize = new Size(1400, 800);

            // Ativa double buffering no Form inteiro para reduzir flicker.
            DoubleBuffered = true;

            // Cria os painéis principais.
            Panel statusBar = CriarBarraStatus();       // Barra de status fixa no fundo.
            painelControles = CriarBarraControles();    // Painel flutuante de controles.

            // Painel do universo (ocupa todo o espaço restante).
            painelUniverso = new PainelDuploBuffer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            painelUniverso.Paint += PainelUniverso_Paint;          // Evento de desenho.
            painelUniverso.Resize += (_, _) => painelUniverso.Invalidate(); // Redesenha ao redimensionar.

            // ORDEM de adição dos controles:
            // 1) statusBar no Bottom
            // 2) painelUniverso com Fill (ocupa o resto)
            // 3) painelControles por último, flutuando por cima (BringToFront)
            Controls.Add(statusBar);
            Controls.Add(painelUniverso);
            Controls.Add(painelControles);

            // Posiciona o painel de controles no canto inferior direito,
            // logo acima da statusBar.
            ReposicionarPainelControles(statusBar);

            // Garante que o painel de controles fique visível por cima.
            painelControles.BringToFront();

            // Redimensiona a posição do painel de controles quando a janela muda de tamanho.
            Resize += (_, _) => ReposicionarPainelControles(statusBar);

            painelUniverso.TabStop = false; // Impede que o painel receba foco via Tab.

            // Timer com intervalo de 35 ms (~28 FPS) para atualizar a simulação.
            timer = new System.Windows.Forms.Timer { Interval = 35 };
            timer.Tick += Timer_Tick;

            // Libera os recursos GDI+ quando o formulário for fechado.
            FormClosed += (_, _) =>
            {
                timer.Stop();

                fonteCorpo.Dispose();
                pincelFundo.Dispose();
                pincelColidiu.Dispose();
                pincelMassivo.Dispose();
                pincelNormal.Dispose();
                canetaContorno.Dispose();
                canetaGrade.Dispose();
            };

            // Cria o universo inicial com os valores padrão dos controles.
            CriarUniversoInicial();
        }

        // Recalcula a posição do painel flutuante de controles para mantê-lo
        // no canto inferior direito, acima da barra de status.
        private void ReposicionarPainelControles(Panel statusBar)
        {
            painelControles.Location = new Point(
                ClientSize.Width - painelControles.Width - 15,
                ClientSize.Height - statusBar.Height - painelControles.Height - 15
            );
        }

        // Cria o painel compacto e flutuante que contém os controles da simulação.
        private Panel CriarBarraControles()
        {
            // Painel compacto, com borda e fundo cinza claro.
            Panel barra = new Panel
            {
                Size = new Size(620, 100),
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            // FlowLayoutPanel organiza os controles automaticamente,
            // quebrando linha quando necessário.
            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = false,
                Padding = new Padding(0)
            };

            // Helper para criar labels compactos.
            Label CriarLabel(string texto) => new Label
            {
                Text = texto,
                ForeColor = Color.Black,
                AutoSize = true,
                Margin = new Padding(4, 8, 0, 0)
            };

            // NumericUpDown para quantidade de corpos.
            nudCorpos = new NumericUpDown
            {
                Minimum = 2,
                Maximum = 1000,
                Value = 6,
                Width = 50,
                ThousandsSeparator = true,
                Margin = new Padding(2, 5, 6, 0)
            };

            // NumericUpDown para quantidade de iterações.
            nudIteracoes = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 100000,
                Value = 1000,
                Width = 65,
                ThousandsSeparator = true,
                Margin = new Padding(2, 5, 6, 0)
            };

            // NumericUpDown para o tempo entre iterações (em segundos).
            nudTempo = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 1000000,
                Value = 10000,
                Width = 70,
                Increment = 100,
                ThousandsSeparator = true,
                Margin = new Padding(2, 5, 6, 0)
            };

            // NumericUpDown para a massa mínima (em unidades de 10²² kg).
            nudMassaMinima = new NumericUpDown
            {
                Minimum = 0.01M,
                Maximum = 100000M,
                Value = 2,
                DecimalPlaces = 2,
                Increment = 0.5M,
                Width = 65,
                Margin = new Padding(2, 5, 6, 0)
            };

            // NumericUpDown para a massa máxima (em unidades de 10²² kg).
            nudMassaMaxima = new NumericUpDown
            {
                Minimum = 0.01M,
                Maximum = 100000M,
                Value = 8,
                DecimalPlaces = 2,
                Increment = 0.5M,
                Width = 65,
                Margin = new Padding(2, 5, 6, 0)
            };

            // Botões compactos.
            btnNovo = CriarBotao("Novo", 60);
            btnIniciar = CriarBotao("Iniciar", 60);
            btnParar = CriarBotao("Parar", 60);
            btnSalvarEstado = CriarBotao("Salvar", 60);
            btnCarregarEstado = CriarBotao("Abrir", 60);

            // Primeira linha: labels e inputs de parâmetros.
            flow.Controls.Add(CriarLabel("Corpos:"));
            flow.Controls.Add(nudCorpos);
            flow.Controls.Add(CriarLabel("Iterações:"));
            flow.Controls.Add(nudIteracoes);
            flow.Controls.Add(CriarLabel("Tempo (s):"));
            flow.Controls.Add(nudTempo);
            flow.Controls.Add(CriarLabel("Massa mín.:"));
            flow.Controls.Add(nudMassaMinima);
            flow.Controls.Add(CriarLabel("Massa máx.:"));
            flow.Controls.Add(nudMassaMaxima);

            // Segunda linha: botões (o WrapContents quebra a linha automaticamente).
            flow.Controls.Add(btnNovo);
            flow.Controls.Add(btnIniciar);
            flow.Controls.Add(btnParar);
            flow.Controls.Add(btnSalvarEstado);
            flow.Controls.Add(btnCarregarEstado);

            // Associa os eventos de clique aos métodos correspondentes.
            btnNovo.Click += BtnNovo_Click;
            btnIniciar.Click += BtnIniciar_Click;
            btnParar.Click += (_, _) => timer.Stop();
            btnSalvarEstado.Click += BtnSalvarEstado_Click;
            btnCarregarEstado.Click += BtnCarregarEstado_Click;

            barra.Controls.Add(flow);
            return barra;
        }

        // Cria a barra de status fixa no rodapé do formulário.
        private Panel CriarBarraStatus()
        {
            Panel barra = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Label que mostra iteração, quantidade de corpos e delta t.
            lblStatus = new Label
            {
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(12, 7)
            };

            // Label que indica se houve colisão.
            lblColisao = new Label
            {
                ForeColor = Color.OrangeRed,
                AutoSize = true,
                Location = new Point(12, 28)
            };

            barra.Controls.Add(lblStatus);
            barra.Controls.Add(lblColisao);
            return barra;
        }

        // Método auxiliar para criar botões padronizados.
        private static Button CriarBotao(string texto, int largura)
        {
            return new Button
            {
                Text = texto,
                Width = largura,
                Height = 28,
                ForeColor = Color.Black,
                BackColor = Color.White,
                Margin = new Padding(4, 4, 4, 0)
            };
        }

        // Cria um novo universo com os parâmetros atuais dos controles.
        private void CriarUniversoInicial()
        {
            try
            {
                // Converte os valores dos NumericUpDown para as unidades corretas.
                double massaMinima = (double)nudMassaMinima.Value * 1.0E22;
                double massaMaxima = (double)nudMassaMaxima.Value * 1.0E22;

                // Chama o controller para criar o universo.
                controller.CriarUniverso(
                    (int)nudCorpos.Value,
                    (int)nudIteracoes.Value,
                    (double)nudTempo.Value,
                    massaMinima,
                    massaMaxima);

                // Salva o estado inicial em arquivo.
                controller.Salvar(arquivoInicial);

                // Redesenha o painel e atualiza a barra de status.
                painelUniverso.Invalidate();
                AtualizarStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Evento do botão "Novo": para a simulação e recria o universo.
        private void BtnNovo_Click(object? sender, EventArgs e)
        {
            timer.Stop();
            CriarUniversoInicial();
        }

        // Evento do botão "Iniciar": inicia o timer da simulação.
        private void BtnIniciar_Click(object? sender, EventArgs e)
        {
            // Se ainda não existe universo, cria um.
            if (controller.UniversoAtual is null)
                CriarUniversoInicial();

            timer.Start();
        }

        // Evento disparado a cada tick do timer.
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Se a simulação terminou, para o timer.
                if (controller.Terminou())
                {
                    timer.Stop();
                }
                else
                {
                    // Executa uma iteração da simulação.
                    controller.ExecutarIteracao();
                }

                // Redesenha o painel e atualiza a barra de status.
                painelUniverso.Invalidate();
                AtualizarStatus();
            }
            catch (Exception ex)
            {
                timer.Stop();
                MessageBox.Show(ex.Message, "Erro durante a simulação",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Evento do botão "Salvar": abre diálogo para salvar o estado atual.
        private void BtnSalvarEstado_Click(object? sender, EventArgs e)
        {
            try
            {
                using SaveFileDialog dialogo = new SaveFileDialog
                {
                    Title = "Salvar estado atual",
                    Filter = "Arquivo de texto (*.txt)|*.txt",
                    DefaultExt = "txt",
                    AddExtension = true,
                    FileName = "universo.txt"
                };

                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    controller.Salvar(dialogo.FileName);
                    MessageBox.Show("Estado salvo com sucesso!", "Salvar",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Evento do botão "Abrir": carrega um estado salvo anteriormente.
        private void BtnCarregarEstado_Click(object? sender, EventArgs e)
        {
            try
            {
                timer.Stop(); // Para a simulação antes de carregar.
                using OpenFileDialog dialogo = new OpenFileDialog
                {
                    Title = "Abrir estado do universo",
                    Filter = "Arquivo de texto (*.txt)|*.txt"
                };

                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    controller.Carregar(dialogo.FileName);
                    painelUniverso.Invalidate();
                    AtualizarStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro ao carregar",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Atualiza os textos da barra de status com informações do universo.
        private void AtualizarStatus()
        {
            Universo? universo = controller.UniversoAtual;
            if (universo is null) return;

            int iteracao = controller.IteracaoVisual;
            int total = universo.QuantidadeIteracoes;

            lblStatus.Text =
                $"Iteração: {iteracao}/{total}    " +
                $"Corpos: {universo.Corpos.Count}    " +
                $"Δt: {universo.TempoEntreIteracoes:N0} s";

            // Verifica se algum corpo colidiu para exibir aviso.
            bool houveColisao = universo.Corpos.Any(corpo => corpo.Colidir);
            lblColisao.Text = houveColisao ? "COLISÃO DETECTADA" : "";
        }

        // Evento de desenho do painel do universo.
        private void PainelUniverso_Paint(object? sender, PaintEventArgs e)
        {
            // Suaviza as bordas dos círculos.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Universo? universo = controller.UniversoAtual;
            if (universo is null) return;

            // Calcula a escala para caber o mundo dentro do painel,
            // mantendo a proporção (letterboxing).
            double escalaX = painelUniverso.ClientSize.Width / UniverseController.LarguraMundo;
            double escalaY = painelUniverso.ClientSize.Height / UniverseController.AlturaMundo;
            double escala = Math.Min(escalaX, escalaY);

            // Calcula os offsets para centralizar o mundo no painel.
            double offsetX = (painelUniverso.ClientSize.Width - UniverseController.LarguraMundo * escala) / 2.0;
            double offsetY = (painelUniverso.ClientSize.Height - UniverseController.AlturaMundo * escala) / 2.0;

            // Preenche o fundo e desenha a grade.
            e.Graphics.FillRectangle(pincelFundo, painelUniverso.ClientRectangle);
            DesenharGrade(e.Graphics);

            // Desenha cada corpo.
            foreach (Corpo corpo in universo.Corpos)
            {
                // Converte a posição do mundo para coordenadas de tela.
                float x = (float)(offsetX + corpo.PosX * escala);
                float y = (float)(offsetY + corpo.PosY * escala);

                // Calcula o raio visual: pelo menos 2 pixels, ou baseado no raio físico
                // e também um raio mínimo proporcional à massa (para corpos muito pequenos).
                float raioFisico = (float)Math.Max(2.0, corpo.CalcularRaio() * escala);
                float raioVisualPorMassa = (float)(6.0 * Math.Cbrt(corpo.Massa / 1.0E22));
                float raio = Math.Max(raioFisico, raioVisualPorMassa);

                // Escolhe o pincel de acordo com o estado do corpo.
                Brush pincel = corpo.Colidir ? pincelColidiu
                             : corpo.Massa >= 5.0E22 ? pincelMassivo
                             : pincelNormal;

                // Desenha o círculo preenchido e o contorno.
                e.Graphics.FillEllipse(pincel, x - raio, y - raio, raio * 2, raio * 2);
                e.Graphics.DrawEllipse(canetaContorno, x - raio, y - raio, raio * 2, raio * 2);

                // Desenha o nome e a massa ao lado do corpo.
                e.Graphics.DrawString(
                    $"{corpo.Nome}\nM={corpo.Massa / 1.0E22:0.##}×10²² kg",
                    fonteCorpo,
                    Brushes.Black,
                    x + raio + 3,
                    y - 16);
            }
        }

        // Desenha uma grade de fundo no painel do universo.
        private void DesenharGrade(Graphics graphics)
        {
            const int espacamento = 80; // Distância entre linhas da grade em pixels.

            // Linhas verticais.
            for (int x = 0; x < painelUniverso.ClientSize.Width; x += espacamento)
                graphics.DrawLine(canetaGrade, x, 0, x, painelUniverso.ClientSize.Height);

            // Linhas horizontais.
            for (int y = 0; y < painelUniverso.ClientSize.Height; y += espacamento)
                graphics.DrawLine(canetaGrade, 0, y, painelUniverso.ClientSize.Width, y);
        }
    }
}
