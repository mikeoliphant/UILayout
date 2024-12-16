using System;
using UILayout;

using var host = new MonoGameTest.TestGameHost(800, 600, isFullscreen: false);

host.UseEmbeddedResources = true;
host.UsePremultipliedAlpha = false; // Because our embedded assets are not pre-multiplied

MonoGameLayout layout = new MonoGameLayout();

host.StartGame(layout);
