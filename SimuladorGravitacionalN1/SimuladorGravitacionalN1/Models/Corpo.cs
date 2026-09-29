using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorGravitacionalN1.Models
{
    public class Corpo
    {
        private const double DensidadeMinima = 0.1; // Densidade mínima em g/cm³
        private const double DensidadadeMaxima = 22500.0; // Densidade máxima em g/cm³ 

        // Propriedades privadas do corpo, incluindo nome, massa, densidade, posição e velocidade.
        private string nome;
        private double massa;
        private double densidade;
        private double posX;
        private double posY;
        private double velX;
        private double velY;

        // Propriedades públicas para acessar os valores privados do corpo.
        public string Nome => nome;
        public double Massa => massa;
        public double Densidade => densidade;
        public double PosX => posX;
        public double PosY => posY;
        public double VelX => velX;
        public double VelY => velY;

        // Propriedade pública para indicar se o corpo colidiu com outro corpo.
        public bool Colidir { get; private set; }

        // Construtor da classe Corpo, que inicializa os valores das propriedades com base nos parâmetros fornecidos.
        public Corpo(string nome, double massa, double densidade, double posX, double posY, double velX, double velY)
        {
            // Validações para garantir que os parâmetros fornecidos sejam válidos.
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("O nome do corpo não pode ser nulo ou vazio.");
            }
            // Valida se a massa é maior que zero
            if (massa <= 0)
            {
                throw new ArgumentException("A massa do corpo deve ser maior que zero.");
            }
            // Valida se a densidade está dentro do intervalo permitido
            if (densidade < DensidadeMinima || densidade > DensidadadeMaxima)
            {
                throw new ArgumentOutOfRangeException($"A densidade deve estar entre {DensidadeMinima} g/cm³ e {DensidadadeMaxima} g/cm³.");
            }

            
            this.nome = nome;
            this.massa = massa;
            this.densidade = densidade;
            this.posX = posX;
            this.posY = posY;
            this.velX = velX;
            this.velY = velY;
            Colidir = false; // Inicializa a propriedade Colidir como false
        }

        public double CalcularVolume() => massa / densidade; // Volume = Massa / Densidade

        public double CalcularRaio() // Raio = (3 * Volume / (4 * PI))^(1/3)
        {
            double volume = CalcularVolume();
            return Math.Pow((3 * volume) / (4 * Math.PI), 1.0 / 3.0);
        }  

        public double CalcularMomentoX() => massa* velX;   // Momento linear na direção X = Massa * VelocidadeX
        public double CalcularMomentoY() => massa * velY; // Momento linear na direção Y = Massa * VelocidadeY

        public void AtualizarPosicao(double deltaTime) // Atualiza a posição do corpo com base na velocidade e no tempo decorrido
        {
            posX += velX * deltaTime;
            posY += velY * deltaTime;
        }
        public void DefinirPosicao(double novaposX, double novaposY) // Define a nova posição do corpo
        {
            posX = novaposX;
            posY = novaposY;
        }
        public void DefinirVelocidade(double novavelX, double novavelY) // Define a nova velocidade do corpo
        {
            velX = novavelX;
            velY = novavelY;
        }
        
        public void MarcarColisao() // Marca o corpo como colidido
        {
            Colidir = true;
        }

        public void LimparColisao() // Limpa a marcação de colisão do corpo
        {
            Colidir = false;
        }  
    }
}
