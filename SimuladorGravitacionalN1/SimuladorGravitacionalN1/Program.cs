using SimuladorGravitacionalN1.Controllers;
using SimuladorGravitacionalN1.Repositories;
using SimuladorGravitacionalN1.Views;

namespace SimuladorGravitacionalN1
{
    // Classe estática que contém o ponto de entrada da aplicação.
    internal static class Program
    {
        // [STAThread] indica que o modelo de threading é Single-Threaded Apartment.
        // É obrigatório para aplicações Windows Forms, pois controles visuais
        // (como diálogos e clipboard) exigem esse modelo para funcionar corretamente.
        [STAThread]
        private static void Main()
        {
            // Habilita os estilos visuais do Windows (temas modernos nos controles).
            Application.EnableVisualStyles();

            // Define que os controles usarão o renderizador de texto compatível
            // (necessário para manter consistência visual em versões antigas do Windows).
            Application.SetCompatibleTextRenderingDefault(false);

            // Cria uma instância concreta do gravador de arquivo texto.
            // A variável é do tipo abstrato GravadorAbstrato, permitindo trocar
            // a implementação (ex: gravador XML, JSON) sem alterar o resto do código.
            GravadorAbstrato gravador = new GravadorArquivoTexto();

            // Cria o Controller passando o gravador através da classe abstrata.
            // Isso é uma aplicação do princípio de inversão de dependência:
            // o Controller não conhece o tipo concreto, apenas a abstração.
            UniverseController controller = new UniverseController(gravador);

            // Inicia a aplicação criando o formulário principal e passando o controller.
            // Application.Run bloqueia até o formulário ser fechado.
            Application.Run(new FormUniverse(controller));
        }
    }
}
