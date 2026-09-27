using SimuladorGravitacionalN1.Controllers;
using SimuladorGravitacionalN1.Repositories;
using SimuladorGravitacionalN1.Views;

namespace SimuladorGravitacionalN1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Utiliza o gravador de arquivo texto.
            GravadorAbstrato gravador = new GravadorArquivoTexto();

            // O Controller recebe o gravador através
            // da classe abstrata.
            UniverseController controller = new UniverseController(gravador);

            Application.Run(new FormUniverse(controller));
        }
    }
}