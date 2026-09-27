using System.Drawing.Drawing2D;
using SimuladorGravitacionalN1.Controllers;
using SimuladorGravitacionalN1.Models;

namespace SimuladorGravitacionalN1.Views
{
    internal class FormUniverse : Form
    {
        private readonly UniverseController controller;

        private Panel painelUniverso = null!;
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
        private System.Windows.Forms.Timer timer = null!;

        private readonly string arquivoInicial;

        public FormUniverse(UniverseController controller)
        {
            this.controller = controller;
            arquivoInicial = Path.Combine(AppContext.BaseDirectory, "posicao_inicial.txt");

            Text = "Simulador Gravitacional 2D - MVC";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1200, 760);
            ClientSize = new Size(1400, 800);

            Panel barraSuperior = CriarBarraControles();
            Controls.Add(barraSuperior);

            painelUniverso = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };
            painelUniverso.Paint += PainelUniverso_Paint;
            painelUniverso.Resize += (_, _) => painelUniverso.Invalidate();
            Controls.Add(painelUniverso);
            painelUniverso.BringToFront();

            Panel statusBar = CriarBarraStatus();
            Controls.Add(statusBar);
            statusBar.BringToFront();

            timer = new System.Windows.Forms.Timer { Interval = 35 };
            timer.Tick += Timer_Tick;
            FormClosed += (_, _) => timer.Stop();

            CriarUniversoInicial();
        }

        private Panel CriarBarraControles()
        {
            Panel barra = new Panel
            {
                Dock = DockStyle.Top,
                Height = 112,
                BackColor = Color.FromArgb(35, 35, 35),
                Padding = new Padding(10)
            };

            Label CriarLabel(string texto, int x, int y)
            {
                return new Label
                {
                    Text = texto,
                    ForeColor = Color.White,
                    Location = new Point(x, y),
                    AutoSize = true
                };
            }

            nudCorpos = new NumericUpDown
            {
                Minimum = 2,
                Maximum = 100,
                Value = 6,
                Location = new Point(70, 8),
                Width = 55,
                ThousandsSeparator = true
            };

            nudIteracoes = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 100000,
                Value = 1000,
                Location = new Point(265, 8),
                Width = 75,
                ThousandsSeparator = true
            };

            nudTempo = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 1000000,
                Value = 10000,
                Location = new Point(455, 8),
                Width = 85,
                Increment = 100,
                ThousandsSeparator = true
            };

            // Massa em unidades de 10²² kg para não precisar digitar números enormes.
            nudMassaMinima = new NumericUpDown
            {
                Minimum = 0.01M,
                Maximum = 100000M,
                Value = 2,
                DecimalPlaces = 2,
                Increment = 0.5M,
                Location = new Point(660, 8),
                Width = 85
            };

            nudMassaMaxima = new NumericUpDown
            {
                Minimum = 0.01M,
                Maximum = 100000M,
                Value = 8,
                DecimalPlaces = 2,
                Increment = 0.5M,
                Location = new Point(865, 8),
                Width = 85
            };

            btnNovo = CriarBotao("Novo", 975, 5, 80);
            btnIniciar = CriarBotao("Iniciar", 1060, 5, 80);
            btnParar = CriarBotao("Parar", 1145, 5, 80);

            btnSalvarEstado = CriarBotao("Salvar estado", 10, 55, 105);
            btnCarregarEstado = CriarBotao("Abrir estado", 120, 55, 105);

            barra.Controls.Add(CriarLabel("Corpos: ", 10, 12));
            barra.Controls.Add(nudCorpos);
            barra.Controls.Add(CriarLabel("Iterações: ", 155, 12));
            barra.Controls.Add(nudIteracoes);
            barra.Controls.Add(CriarLabel("Tempo (s): ", 360, 12));
            barra.Controls.Add(nudTempo);
            barra.Controls.Add(CriarLabel("Massa mín.(10²²kg):", 550, 12));
            barra.Controls.Add(nudMassaMinima);
            barra.Controls.Add(CriarLabel("Massa máx.(10²²kg): ", 755, 12));
            barra.Controls.Add(nudMassaMaxima);

            barra.Controls.Add(btnNovo);
            barra.Controls.Add(btnIniciar);
            barra.Controls.Add(btnParar);
            barra.Controls.Add(btnSalvarEstado);
            barra.Controls.Add(btnCarregarEstado);

            btnNovo.Click += BtnNovo_Click;
            btnIniciar.Click += BtnIniciar_Click;
            btnParar.Click += (_, _) => timer.Stop();
            btnSalvarEstado.Click += BtnSalvarEstado_Click;
            btnCarregarEstado.Click += BtnCarregarEstado_Click;

            return barra;
        }

        private Panel CriarBarraStatus()
        {
            Panel barra = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.FromArgb(35, 35, 35)
            };

            lblStatus = new Label
            {
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(12, 7)
            };

            lblColisao = new Label
            {
                ForeColor = Color.Orange,
                AutoSize = true,
                Location = new Point(12, 28)
            };

            barra.Controls.Add(lblStatus);
            barra.Controls.Add(lblColisao);
            return barra;
        }

        private static Button CriarBotao(string texto, int x, int y, int largura)
        {
            return new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Width = largura,
                Height = 34,
                ForeColor = Color.White
            };
        }

        private void CriarUniversoInicial()
        {
            try
            {
                double massaMinima = (double)nudMassaMinima.Value * 1.0E22;
                double massaMaxima = (double)nudMassaMaxima.Value * 1.0E22;

                controller.CriarUniverso(
                    (int)nudCorpos.Value,
                    (int)nudIteracoes.Value,
                    (double)nudTempo.Value,
                    massaMinima,
                    massaMaxima);

                controller.Salvar(arquivoInicial);
                painelUniverso.Invalidate();
                AtualizarStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNovo_Click(object? sender, EventArgs e)
        {
            timer.Stop();
            CriarUniversoInicial();
        }

        private void BtnIniciar_Click(object? sender, EventArgs e)
        {
            if (controller.UniversoAtual is null)
                CriarUniversoInicial();

            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (controller.Terminou())
                {
                    timer.Stop();
                }
                else
                {
                    controller.ExecutarIteracao();
                }

                painelUniverso.Invalidate();
                AtualizarStatus();
            }
            catch (Exception ex)
            {
                timer.Stop();
                MessageBox.Show(ex.Message, "Erro durante a simulação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
                    MessageBox.Show("Estado salvo com sucesso!", "Salvar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCarregarEstado_Click(object? sender, EventArgs e)
        {
            try
            {
                timer.Stop();
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
                MessageBox.Show(ex.Message, "Erro ao carregar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AtualizarStatus()
        {
            Universo? universo = controller.UniversoAtual;

            if (universo is null)
                return;

            int iteracao = controller.IteracaoVisual;
            int total = universo.QuantidadeIteracoes;

            lblStatus.Text =
                $"Iteração: {iteracao}/{total}    " +
                $"Corpos: {universo.Corpos.Count}    " +
                $"Δt: {universo.TempoEntreIteracoes:N0} s";

            bool houveColisao = universo.Corpos.Any(corpo => corpo.Colidir);

            lblColisao.Text = houveColisao ? "COLISÃO DETECTADA" : "";
        }

        private void PainelUniverso_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Universo? universo = controller.UniversoAtual;
            if (universo is null)
                return;

            double escalaX = painelUniverso.ClientSize.Width / UniverseController.LarguraMundo;
            double escalaY = painelUniverso.ClientSize.Height / UniverseController.AlturaMundo;
            double escala = Math.Min(escalaX, escalaY);

            double offsetX = (painelUniverso.ClientSize.Width - UniverseController.LarguraMundo * escala) / 2.0;
            double offsetY = (painelUniverso.ClientSize.Height - UniverseController.AlturaMundo * escala) / 2.0;

            using Brush fundo = new SolidBrush(Color.FromArgb(10, 10, 18));
            e.Graphics.FillRectangle(fundo, painelUniverso.ClientRectangle);
            DesenharGrade(e.Graphics);

            foreach (Corpo corpo in universo.Corpos)
            {
                float x = (float)(offsetX + corpo.PosX * escala);
                float y = (float)(offsetY + corpo.PosY * escala);

                // A física usa o raio SI calculado por massa+densiade.
                // Para ficar visível, a View usa também a massa para dar escala visual.
                float raioFisico = (float)Math.Max(2.0, corpo.CalcularRaio() * escala);
                float raioVisualPorMassa = (float)(6.0 * Math.Cbrt(corpo.Massa / 1.0E22));
                float raio = Math.Max(raioFisico, raioVisualPorMassa);

                Brush pincel;
                if (corpo.Colidir)
                    pincel = Brushes.OrangeRed;
                else if (corpo.Massa >= 5.0E22)
                    pincel = Brushes.DeepSkyBlue;
                else
                    pincel = Brushes.LightSkyBlue;

                e.Graphics.FillEllipse(pincel, x - raio, y - raio, raio * 2, raio * 2);
                e.Graphics.DrawEllipse(Pens.White, x - raio, y - raio, raio * 2, raio * 2);

                using Font fonte = new Font("Segoe UI", 8, FontStyle.Bold);
                e.Graphics.DrawString(
                    $"{corpo.Nome}\nM={corpo.Massa / 1.0E22:0.##}×10²² kg",
                    fonte,
                    Brushes.White,
                    x + raio + 3,
                    y - 16);
            }
        }

        private void DesenharGrade(Graphics graphics)
        {
            using Pen pen = new Pen(Color.FromArgb(35, 35, 50), 1);
            const int espacamento = 80;

            for (int x = 0; x < painelUniverso.ClientSize.Width; x += espacamento)
                graphics.DrawLine(pen, x, 0, x, painelUniverso.ClientSize.Height);

            for (int y = 0; y < painelUniverso.ClientSize.Height; y += espacamento)
                graphics.DrawLine(pen, 0, y, painelUniverso.ClientSize.Width, y);
        }
    }
}