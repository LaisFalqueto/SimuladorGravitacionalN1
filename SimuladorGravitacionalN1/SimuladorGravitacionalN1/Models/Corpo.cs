using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorGravitacionalN1.Models
{
    public class Corpo
    {
        private const double DensidadeMinima = 0.1; // Densidade mínima em g/cm³
        private const double DensidadadeMaxima = 22500.0; // Densidade máxima em g/cm³ 

        private string nome;
        private double massa;
        private double densidade;
        private double posX;
        private double posY;
        private double velX;
        private double velY;

        public string Nome => nome;
        public double Massa => massa;
        public double Densidade => densidade;
        public double PosX => posX;
        public double PosY => posY;
        public double VelX => velX;
        public double VelY => velY;

        public bool Colidir { get; private set; }

        public Corpo(string nome, double massa, double densidade, double posX, double posY, double velX, double velY)
        {
            if(string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("O nome do corpo não pode ser nulo ou vazio.");
            }
            if (massa <= 0)
            {
                throw new ArgumentException("A massa do corpo deve ser maior que zero.");
            }
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

        public double CalcularVolume() => massa / densidade;

        public double CalcularRaio()
        {
            double volume = CalcularVolume();
            return Math.Pow((3 * volume) / (4 * Math.PI), 1.0 / 3.0);
        }  

        public double CalcularMomentoX() => massa* velX;
        public double CalcularMomentoY() => massa * velY;

        public void AtualizarPosicao(double deltaTime)
        {
            posX += velX * deltaTime;
            posY += velY * deltaTime;
        }
        public void DefinirPosicao(double novaposX, double novaposY)
        {
            posX = novaposX;
            posY = novaposY;
        }
        public void DefinirVelocidade(double novavelX, double novavelY)
        {
            velX = novavelX;
            velY = novavelY;
        }
        
        public void MarcarColisao()
        {
            Colidir = true;
        }

        public void LimparColisao()
        {
            Colidir = false;
        }  
    }
}
