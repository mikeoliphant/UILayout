using System;
using UILayout;

using var host = new MonoGameTest.TestGameHost(800, 600, isFullscreen: false);

host.UseEmbeddedResources = true;

MonoGameLayout layout = new MonoGameLayout();

host.StartGame(layout);
