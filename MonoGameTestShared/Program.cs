using System;

namespace UILayout
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using var host = new MonoGameTest.TestGameHost(800, 600, isFullscreen: false);

            MonoGameLayout layout = new MonoGameLayout();

            host.StartGame(layout);
        }
    }
}