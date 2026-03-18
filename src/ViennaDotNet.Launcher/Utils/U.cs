namespace ViennaDotNet.Launcher.Utils;

internal static class U
{
    public static void PAK(string text = "continue")
    {
        Console.WriteLine($"Press any key to {text}...");
        Console.ReadKey(true);
    }

    public static void ConfirmType(string textToType)
    {
        textToType = textToType.ToLowerInvariant();

        Console.WriteLine($"Type \"{textToType}\" (without \") and hit enter to proceed");

        while (true)
        {
            string typed = Console.ReadLine()?.ToLowerInvariant() ?? string.Empty;

            if (typed == textToType)
                return;

            Console.WriteLine("Incorrect input. Try again.");
        }
    }
}
