using System.Text;

namespace Common;

internal static class IO
{
    /// <summary>
    /// Construct, and print, a message in the console where the given value is highlighted
    /// </summary>
    /// <param name="value">The value of interest</param>
    /// <param name="prefix">Prefix text before the highlight</param>
    /// <param name="suffix">Suffix text after the highlight</param>
    /// <param name="spaceAfter">Whether or not to add a space after the highlighted text</param>
    /// <param name="spaceBefore">Whether or not to add a space before the highlighted text</param>
    public static void WriteResult<T>(this T value, string prefix = "", string suffix = "", bool spaceAfter = true, bool spaceBefore = true) =>
        WriteResult(value.ToString(), prefix, suffix, spaceAfter, spaceBefore);

    /// <summary>
    /// Construct, and print, a message in the console where the given value is highlighted
    /// </summary>
    /// <param name="message">The part of the message that should be highlighted</param>
    /// <param name="prefix">Prefix text before the highlight</param>
    /// <param name="suffix">Suffix text after the highlight</param>
    /// <param name="spaceAfter">Whether or not to add a space after the highlighted text</param>
    /// <param name="spaceBefore">Whether or not to add a space before the highlighted text</param>
    public static void WriteResult(this string message, string prefix = "", string suffix = "", bool spaceAfter = true, bool spaceBefore = true)
    {
        if (prefix.Length > 0)
        {   // Add prefix to message, and a space, if requested
            Console.Write(prefix);
            if (spaceBefore) Console.Write(' ');
        }

        Write(message, Colors.Positive);

        if (suffix.Length > 0)
        {   // Add suffix to message, and a space, if requested
            if (spaceAfter) Console.Write(' ');
            Console.Write(suffix);
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Write debug information in the console, which will be overwritten after additional writes
    /// </summary>
    /// <param name="value">The value of interest</param>
    /// <param name="prefix">Prefix text before the value</param>
    /// <param name="suffix">Suffix text after the value</param>
    /// <param name="spaceAfter">Whether or not to add a space after the value text</param>
    /// <param name="spaceBefore">Whether or not to add a space before the value text</param>
    public static void WriteDebug<T>(this T value, string prefix = "", string suffix = "", bool spaceAfter = true, bool spaceBefore = true) =>
        WriteDebug(value.ToString(), prefix, suffix, spaceAfter, spaceBefore);

    /// <summary>
    /// Write debug information in the console, which will be overwritten after additional writes
    /// </summary>
    /// <param name="message">The value of interest</param>
    /// <param name="prefix">Prefix text before the value</param>
    /// <param name="suffix">Suffix text after the value</param>
    /// <param name="spaceAfter">Whether or not to add a space after the value text</param>
    /// <param name="spaceBefore">Whether or not to add a space before the value text</param>
    public static void WriteDebug(this string message, string prefix = "", string suffix = "", bool spaceAfter = true, bool spaceBefore = true)
    {
        var debugMessage = new StringBuilder();

        if (prefix.Length > 0)
        {   // Add prefix to message, and a space, if requested
            debugMessage.Append(prefix);
            if (spaceBefore) debugMessage.Append(' ');
        }

        debugMessage.Append(message);

        if (suffix.Length > 0)
        {   // Add suffix to message, and a space, if requested
            if (spaceAfter) debugMessage.Append(' ');
            debugMessage.Append(suffix);
        }

        Write(debugMessage.ToString(), Colors.Debug);
        Console.CursorLeft = 0;
    }

    /// <summary>
    /// Write an error in the console
    /// </summary>
    /// <param name="error">The error to write</param>
    /// <param name="pause">Whether to pause execution until user confirms</param>
    public static void WriteError(this string error, bool pause = true)
    {
        Write(error, Colors.Error);
        if (pause) Console.ReadLine();
    }

    /// <summary>
    /// Print the time elapsed since the task started on the right end of the console.
    /// </summary>
    /// <param name="startTime">The time that the task started</param>
    /// <param name="alignmentPoint">The cursor position to align against</param>
    public static void WriteElapsedTime(this DateTime startTime, int alignmentPoint = 0)
    {   // Determine elapsed time
        var elapsedTime = DateTime.Now - startTime;
        var elapsedText = elapsedTime.ToString()[3..];

        // Determine where to start printing the timestamp
        if (alignmentPoint == 0) alignmentPoint = Console.BufferWidth;
        var printLocation = alignmentPoint - elapsedText.Length;
        Console.CursorLeft = printLocation;

        // Print the timestamp
        Write(elapsedText, Colors.Debug);
        Console.CursorLeft = 0;
    }

    /// <summary>
    /// Write a message to the console, with a newline appended
    /// </summary>
    /// <param name="message">The message to write</param>
    /// <param name="color">The color to write the message in</param>
    public static void WriteLine(this string message, ConsoleColor color) =>
        Write($"{message}{Environment.NewLine}", color);

    /// <summary>
    /// Write a message to the console
    /// </summary>
    /// <param name="message">The message to write</param>
    /// <param name="color">The color to write the message in</param>
    public static void Write(this string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = Colors.Default;
    }

    /// <summary>
    /// Clear the line that the cursor is currently on
    /// </summary>
    /// <param name="start">Which character to start at</param>
    /// <param name="length">The number of characters to clear. Default is entire row</param>
    public static void ClearLine(int start = 0, int length = 0)
    {   // If no length is given, clear the remaining width of the console
        if (length == 0) length = Console.BufferWidth - start;

        Console.CursorLeft = start;
        Console.Write(new string(' ', length));
        Console.CursorLeft = 0;
    }

    /// <summary>
    /// Read the contents of the clipboard
    /// </summary>
    /// <returns>The contents of the clipboard</returns>
    public static string[] ReadClipboard()
    {   // Clipboard can only be accessed from a STAThread
        string[] clipboard = null;
        var STAThread = new Thread(() => clipboard = Clipboard.GetText().Split(Environment.NewLine));
        if (OperatingSystem.IsWindows()) STAThread.SetApartmentState(ApartmentState.STA);
        STAThread.Start();
        STAThread.Join();
        return clipboard;
    }

    /// <summary>
    /// Set the contents of the clipboard
    /// </summary>
    /// <param name="contents">What to set the clipboard to</param>
    public static void WriteClipboard(string contents)
    {   // Clipboard can only be accessed from a STAThread
        var STAThread = new Thread(() => Clipboard.SetText(contents));
        if (OperatingSystem.IsWindows()) STAThread.SetApartmentState(ApartmentState.STA);
        STAThread.Start();
        STAThread.Join();

        Write($"Clipboard content has been set to '", Colors.Debug);
        Write(contents, Colors.Positive);
        WriteLine("'", Colors.Debug);
    }
}