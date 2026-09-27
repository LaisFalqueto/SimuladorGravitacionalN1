using SimuladorGravitacionalN1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimuladorGravitacionalN1.Repositories
{
    public abstract class GravadorAbstrato
    {
        // Salva a configuração atual do Universo em um arquivo.
        public abstract void Salvar(
            Universo universo,
            string caminho);

        // Carrega uma configuração anteriormente salva.
        public abstract Universo Carregar(
            string caminho);
    }
}
