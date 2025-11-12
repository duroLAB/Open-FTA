namespace OpenFTA
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            ApplicationConfiguration.Initialize();
            var mainForm = new MainForm();

            /* var screen = Screen.FromControl(mainForm);


             using (var splash = new SplashScreen(screen))
             {
                 splash.Show();
                 splash.Refresh();

                 // Počkáme, kým splash dokončí fade efekt
                 while (!splash.IsReadyToClose)
                 {
                     Application.DoEvents();
                     System.Threading.Thread.Sleep(20);
                 }
             }*/

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            Application.Run(mainForm);
        }
    }
}