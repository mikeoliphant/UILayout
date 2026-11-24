using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using NativeFileDialogNET;

namespace UILayout
{
    public partial class Layout
    {
        public string GetFolder(string initialPath)
        {
            if (UseExternalFileDialogs)
                return ExternalFileDialog.SelectFolder(initialPath);

            using var selectFolderDialog = new NativeFileDialog()
                .SelectFolder();

            string folder;

            var result = selectFolderDialog.Open(out folder, initialPath);

            if (result == DialogResult.Okay)
            {
                return folder;
            }

            return null;
        }

        public string GetFile(string initialPath)
        {
            return GetFile(initialPath, null, null);
        }

        public string GetFile(string initialPath, string patternName, string patternWildcard)
        {
            if (UseExternalFileDialogs)
                return ExternalFileDialog.SelectFile(initialPath, patternName, patternWildcard);

            using var selectFileDialog = new NativeFileDialog()
                .SelectFile();

            if (!string.IsNullOrEmpty(patternName))
                selectFileDialog.AddFilter(patternName, patternWildcard);

            string file;

            var result = selectFileDialog.Open(out file, initialPath);

            if (result == DialogResult.Okay)
            {
                return file;
            }

            return null;
        }

        /// <summary>
        /// Show file dialogs (GetFolder, GetFile) in a separate program, zenity or kdialog (Linux), instead of the native
        /// dialog in this process. The embedded backend sets it on Linux: there the native dialog is GTK, loaded into the
        /// DAW's process, which crashes when opened from an editor's render thread and leaves the DAW's UI unresponsive
        /// even when opened on the DAW's own thread (Reaper).
        /// </summary>
        public bool UseExternalFileDialogs { get; set; }
    }

    /// <summary>
    /// File dialogs shown by a separate program: zenity (GNOME and most desktops), or kdialog (KDE). The call blocks
    /// until the dialog closes. Returns null if the dialog was cancelled or neither program is installed.
    /// </summary>
    static class ExternalFileDialog
    {
        public static string SelectFolder(string initialPath)
        {
            List<string> zenity = ["--file-selection", "--directory"];
            List<string> kdialog = ["--getexistingdirectory"];

            if (!string.IsNullOrEmpty(initialPath))
            {
                zenity.Add("--filename=" + initialPath.TrimEnd('/') + "/"); // the trailing slash starts inside the folder
                kdialog.Add(initialPath);
            }

            return Run(zenity, kdialog);
        }

        public static string SelectFile(string initialPath, string patternName, string patternWildcard)
        {
            List<string> zenity = ["--file-selection"];
            List<string> kdialog = ["--getopenfilename", string.IsNullOrEmpty(initialPath) ? "." : initialPath];

            if (!string.IsNullOrEmpty(initialPath))
                zenity.Add("--filename=" + initialPath);

            if (!string.IsNullOrEmpty(patternName) && !string.IsNullOrEmpty(patternWildcard))
            {
                // Accepts "png,jpg", "*.png;*.jpg" or "*.png *.jpg".
                var patterns = new List<string>();

                foreach (string p in patternWildcard.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries))
                    patterns.Add(p.StartsWith('*') ? p : "*." + p.TrimStart('.'));

                string globs = string.Join(' ', patterns);

                zenity.Add("--file-filter=" + patternName + " | " + globs);
                kdialog.Add(globs + "|" + patternName);
            }

            return Run(zenity, kdialog);
        }

        // Runs zenity, or kdialog if zenity isn't installed. Returns the selected path, or null if cancelled.
        static string Run(List<string> zenityArguments, List<string> kdialogArguments)
        {
            if (TryRun("zenity", zenityArguments, out string path) || TryRun("kdialog", kdialogArguments, out path))
                return path;

            Console.Error.WriteLine("UILayout: no file dialog available: install zenity or kdialog");

            return null;
        }

        // False if the program isn't installed.
        static bool TryRun(string program, List<string> arguments, out string path)
        {
            path = null;

            var startInfo = new ProcessStartInfo(program)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            foreach (string argument in arguments)
                startInfo.ArgumentList.Add(argument);

            Process process;

            try
            {
                process = Process.Start(startInfo);
            }
            catch (Win32Exception)
            {
                return false;
            }

            using (process)
            {
                process.ErrorDataReceived += (sender, e) => { }; // drain GTK/Qt warnings so the pipe can't fill up
                process.BeginErrorReadLine();

                string output = process.StandardOutput.ReadToEnd().Trim();

                process.WaitForExit();

                if (process.ExitCode == 0 && output.Length > 0)
                    path = output;
            }

            return true;
        }
    }
}
