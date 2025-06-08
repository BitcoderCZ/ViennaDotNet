using System.Diagnostics;
using System.Runtime.InteropServices;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.Common;

// from https://stackoverflow.com/a/50311340/15878562
public class ConsoleProcess
{
    private readonly string appName;
    public readonly Process Process = new Process();

    public bool IORedirected { get; private set; }

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

    public int ExitCode
        => Process.ExitCode;

    public int Id
        => Process.Id;

    private bool running;

    public ConsoleProcess(string appName, bool useShellExecute, bool redirect)
    {
        if (redirect && useShellExecute)
            throw new InvalidOperationException($"Can't redirect std in/out when useShellExecute is true");

        this.appName = appName;

        IORedirected = redirect;

        Process.StartInfo = new ProcessStartInfo(appName);
        Process.StartInfo.RedirectStandardError = redirect;

        Process.StartInfo.RedirectStandardInput = redirect;
        Process.StartInfo.RedirectStandardOutput = redirect;
        Process.EnableRaisingEvents = true;
        Process.StartInfo.CreateNoWindow = !useShellExecute;

        Process.StartInfo.UseShellExecute = useShellExecute;

        Process.Exited += ProcessOnExited;
    }

    public void ExecuteAsync(string? workingDir, params string[] args)
    {
        if (running)
            throw new InvalidOperationException("Process is still Running. Please wait for the process to complete.");

        if (!string.IsNullOrEmpty(workingDir))
            Process.StartInfo.WorkingDirectory = workingDir;

        string arguments = string.Join(" ", args);
        Process.StartInfo.Arguments = arguments;

        Process.Start();
        running = true;

        if (IORedirected && Process.StartInfo.UseShellExecute)
            throw new InvalidOperationException($"Can't redirect std in/out when useShellExecute is true");
        else if (IORedirected)
        {
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
        }
    }

    public void Write(string data)
    {
        if (!IORedirected) throw new InvalidOperationException($"Can't write, because {nameof(IORedirected)} is false");

        if (data == null)
            return;

        Process.StandardInput.Write(data);
        Process.StandardInput.Flush();
    }

    public void WriteLine(string data)
    {
        Write(data + Environment.NewLine);
    }

    protected virtual void OnProcessExited()
    {
        ProcessExited?.Invoke(this, EventArgs.Empty);
    }

    private void ProcessOnExited(object? sender, EventArgs eventArgs)
    {
        OnProcessExited();
    }

    public void StopAndWait()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            FreeConsole(); // free back our console
            if (AttachConsole((uint)Process.Id))
            {
                SetConsoleCtrlHandler(null, true);
                try
                {
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                    {
                        Console.WriteLine($"Failed to send Ctrl+C to process");
                        Process.TryStopGracefully();
                        Process.WaitForExit();
                        return;
                    }

                    Process.WaitForExit();
                }
                finally
                {
                    SetConsoleCtrlHandler(null, false);
                    FreeConsole();
                    ConsoleUtils.CreateConsole(true);
                    Console.WriteLine("Re allocated/attached console");
                }

                return;
            }
            else
                Console.WriteLine("Failed to attach to console of another process");
        }

        Process.TryStopGracefully();
        Process.WaitForExit();
    }

    internal const int CTRL_C_EVENT = 0;
    [DllImport("kernel32.dll")]
    internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool AttachConsole(uint dwProcessId);
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    internal static extern bool FreeConsole();
    [DllImport("kernel32.dll")]
    static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? HandlerRoutine, bool Add);
    // Delegate type to be used as the Handler Routine for SCCH
    private delegate bool ConsoleCtrlDelegate(uint CtrlType);
}
