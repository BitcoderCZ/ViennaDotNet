using System.Diagnostics;
using System.Runtime.InteropServices;
using Serilog;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.Common;

// from https://stackoverflow.com/a/50311340/15878562
public sealed class ConsoleProcess
{
    private readonly string _filePath;
    public readonly Process Process = new Process();

    public bool IORedirected { get; private set; }
    public bool OpenInNewWindow { get; private set; }

    public event DataReceivedEventHandler? ErrorTextReceived
    {
        add => Process.ErrorDataReceived += value;
        remove => Process.ErrorDataReceived -= value;
    }
    public event EventHandler? ProcessExited;
    public event DataReceivedEventHandler? StandartTextReceived
    {
        add => Process.OutputDataReceived += value;
        remove => Process.OutputDataReceived -= value;
    }

    public int ExitCode => Process.ExitCode;
    public int Id => Process.Id;

    private bool running;

    public ConsoleProcess(string appName, bool useShellExecute, bool redirect, bool openInNewWindow = false)
    {
        if (openInNewWindow && redirect)
        {
            throw new InvalidOperationException("Standard I/O cannot be redirected when opening in a new window.");
        }

        if (redirect && useShellExecute)
        {
            throw new InvalidOperationException("Can't redirect std in/out when useShellExecute is true");
        }

        _filePath = appName;
        IORedirected = redirect;
        OpenInNewWindow = openInNewWindow;

        Process.StartInfo = new ProcessStartInfo(appName)
        {
            RedirectStandardError = redirect,
            RedirectStandardInput = redirect,
            RedirectStandardOutput = redirect,
            UseShellExecute = useShellExecute,
            CreateNoWindow = !useShellExecute && !openInNewWindow,
        };

        Process.EnableRaisingEvents = true;

        Process.Exited += ProcessOnExited;
    }

    public void ExecuteAsync(string? workingDir, params string[] args)
    {
        if (running)
        {
            throw new InvalidOperationException("Process is still Running. Please wait for the process to complete.");
        }

        if (!string.IsNullOrEmpty(workingDir))
        {
            Process.StartInfo.WorkingDirectory = workingDir;
        }

        var formattedArgs = args.Select(a =>
        {
            if (string.IsNullOrEmpty(a))
            {
                return "\"\"";
            }

            if (a.Contains(" ") || a.Contains("{") || a.Contains("\""))
            {
                return $"\"{a.Replace("\"", "\\\"")}\"";
            }

            return a;
        });

        string arguments = string.Join(" ", formattedArgs);

        if (OpenInNewWindow)
        {
            ApplyTerminalWrapper(arguments);
        }
        else
        {
            Process.StartInfo.Arguments = arguments;
        }

        Process.Start();
        running = true;

        if (IORedirected)
        {
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
        }
    }


    public void Write(string data)
    {
        if (!IORedirected)
        {
            throw new InvalidOperationException($"Can't write, because {nameof(IORedirected)} is false");
        }

        if (data is null)
        {
            return;
        }

        Process.StandardInput.Write(data);
        Process.StandardInput.Flush();
    }

    public void WriteLine(string data)
        => Write(data + Environment.NewLine);

    private void OnProcessExited()
        => ProcessExited?.Invoke(this, EventArgs.Empty);

    private void ProcessOnExited(object? sender, EventArgs eventArgs)
        => OnProcessExited();

    public void StopAndWait(int timeout = 15 * 1000)
        => Process.StopGracefullyOrKill(timeout);

    private void ApplyTerminalWrapper(string formattedArgs)
    {
        Process.StartInfo.UseShellExecute = true;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.StartInfo.FileName = "cmd.exe";

            Process.StartInfo.Arguments = $"/k \"\"{_filePath}\" {formattedArgs}\"";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.StartInfo.FileName = "x-terminal-emulator";
            Process.StartInfo.Arguments = $"-e bash -c \"'{_filePath}' {formattedArgs}; exec bash\"";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string command = $"'{_filePath}' {formattedArgs}";
            string appleScript = $"tell application \"Terminal\" to do script \"{command.Replace("\"", "\\\"")}\"";
            Process.StartInfo.FileName = "osascript";
            Process.StartInfo.Arguments = $"-e \"{appleScript}\"";
        }
    }
}
