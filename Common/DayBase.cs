using Common.Enums;
using System.Collections;
using System.Text;

namespace Common;

public abstract class DayBase
{
    private readonly int _day;
    private readonly int _year;
    private readonly int _headerWidth;
    private DateTime _startTime;

    public DayBase()
    {
        Results = new string[2];
        (_year, _day) = GetDate();
        _headerWidth = PrintHeader(_year, _day);
    }

    protected string[] Results { get; }

    /// <summary>
    /// Get the input, run the task, then handle the results
    /// </summary>
    public void Run()
    {
        if (Client.TryGetInput(_year, _day, out var input))
        {   // Run task with given input
            _startTime = DateTime.Now;

            Run(input);
            HandleResults();
        }
        else // Allow user to read error before exiting
            Console.ReadLine();
    }

    /// <summary>
    /// Run the task
    /// </summary>
    /// <param name="input">The input to use</param>
    protected abstract void Run(string[] input);

    /// <summary>
    /// Handle the result
    /// </summary>
    /// <param name="result">The result to handle</param>
    /// <param name="part">The part the result is for</param>
    protected void PrintResult<T>(T result, Part part = Part.Auto) =>
        PrintResult(string.Empty, result, string.Empty, true, true, part);

    /// <summary>
    /// Handle the result, with the given prefix
    /// </summary>
    /// <typeparam name="T">Any type, except for string</typeparam>
    /// <param name="prefix">Text to write before the result</param>
    /// <param name="result">The result to handle</param>
    /// <param name="part">The part the result is for</param>
    protected void PrintResult<T>(string prefix, T result, Part part = Part.Auto) =>
        PrintResult(prefix, result, string.Empty, true, true, part);

    /// <summary>
    /// Handle the result, with the given suffix
    /// </summary>
    /// <typeparam name="T">Any type, except for string</typeparam>
    /// <param name="result">The result to handle</param>
    /// <param name="suffix">Text to write after the result</param>
    /// <param name="part">The part the result is for</param>
    protected void PrintResult<T>(T result, string suffix, Part part = Part.Auto) =>
        PrintResult(string.Empty, result, suffix, true, true, part);

    /// <summary>
    /// Handle the result, with the given prefix and suffix
    /// </summary>
    /// <param name="prefix">Text to write before the result</param>
    /// <param name="result">The result to handle</param>
    /// <param name="suffix">Text to write after the result</param>
    /// <param name="part">The part the result is for</param>
    protected void PrintResult<T>(string prefix, T result, string suffix, Part part = Part.Auto) =>
        PrintResult(prefix, result, suffix, true, true, part);

    /// <summary>
    /// Handle the result, with the given prefix and suffix
    /// </summary>
    /// <param name="prefix">Text to write before the result</param>
    /// <param name="result">The result to handle</param>
    /// <param name="suffix">Text to write after the result</param>
    /// <param name="spaceAfter">Whether or not to place a space between the result and suffix</param>
    /// <param name="spaceBefore">Whether or not to place a space between the result and prefix</param>
    /// <param name="part">The part the result is for</param>
    protected void PrintResult<T>(string prefix, T result, string suffix, bool spaceAfter, bool spaceBefore, Part part = Part.Auto)
    {
        if (part == Part.Auto)
            // Automatically determine where to store the result
            part = Results[0] == null ? Part.One : Part.Two;

        // Convert the result to a useful format
        var output = SanitizeResult(result);

        // Print the result, and the elapsed time
        IO.WriteElapsedTime(_startTime, _headerWidth);
        output.WriteResult(prefix, suffix, spaceAfter, spaceBefore);

        // Store the result
        Results[(int) part - 1] = output.ToString();
    }

    /// <summary>
    /// Some types of T need additional sanitization before becoming printable
    /// </summary>
    /// <param name="result">The result to print</param>
    /// <returns>The string representation of the result</returns>
    private static string SanitizeResult<T>(T result)
    {   // String requires no changes
        if (result is string s) return s;

        if (result is IEnumerable e)
        {   // IEnumerable.ToString() is not useful, print contents instead
            var builder = new StringBuilder();
            foreach(var element in e)
                builder.Append(element.ToString());
            return builder.ToString();
        }

        // Do no sanitization
        return result.ToString();
    }

    /// <summary>
    /// Print a header for the executing task
    /// </summary>
    /// <param name="year">The year to put in the header</param>
    /// <param name="day">The day to put in the header</param>
    /// <returns>The width of the header</returns>
    private static int PrintHeader(int year, int day)
    {   // Construct header contents
        var content = $" {day} [{year}] ";

        // Construct the delimiter
        var length = Console.BufferWidth / 2 - content.Length / 2;
        var delimiter = new string('-', length);

        // Print the header
        var header = $"{delimiter}{content}{delimiter}";
        header.WriteLine(Colors.Neutral);
        return header.Length;
    }

    /// <summary>
    /// Present options to user, and query how to handle the results
    /// </summary>
    private void HandleResults()
    {
        var optionsStart = Console.CursorTop;
        var options = new List<string>();
        var actions = new List<Action>();
        if (!string.IsNullOrWhiteSpace(Results[0]))
        {   // Determine option for result #1
            var (option, action) = DetermineOption(1);
            options.Add(option);
            actions.Add(action);
        }

        if (!string.IsNullOrWhiteSpace(Results[1]))
        {   // Determine option for result #2
            var (option, action) = DetermineOption(2);
            options.Add(option);
            actions.Add(action);
        }

        if (options.Count > 0)
        {   // Present the options
            Console.WriteLine();
            for (var i = 0; i < options.Count; i++)
                IO.WriteLine($"Press {i + 1} to {options[i]}", Colors.Neutral);
            IO.WriteLine("Press any other key to exit", Colors.Neutral);
        }
        else
            IO.WriteLine("Press any key to exit", Colors.Debug);

        // Query the user
        IO.Write("> ", Colors.Input);
        var selection = Console.ReadKey().KeyChar.ToString();

        while (Console.CursorTop > optionsStart)
        {   // Clear the option menu
            IO.ClearLine();
            Console.CursorTop--;
        }

        Console.CursorTop++;

        if (int.TryParse(selection, out var selected) && selected <= options.Count)
            actions[selected - 1].Invoke();
    }

    /// <summary>
    /// Determine what the option is for the given part
    /// </summary>
    /// <param name="part">The part to generate an option for</param>
    /// <returns>The operation to execute, as well as its description</returns>
    private (string, Action) DetermineOption(int part)
    {
        var result = Results[part - 1];
        var (verb, target, action) =
            Client.HasID switch
            {
                true => ("submit", $"as the answer for part {part}", () => Client.SubmitAnswer(_year, _day, part, result)),
                false => ("copy", "to clipboard", (Action) (() => IO.WriteClipboard(result))),
            };

        return ($"{verb} '{result}' {target}", action);
    }

    /// <summary>
    /// Get the day, and year, of this task. Based on the name of the class, and assembly, respectively
    /// </summary>
    /// <returns>The extracted date</returns>
    /// <exception cref="ArgumentException">The name of either the class or the assembly was invalid</exception>
    private (int, int) GetDate()
    {   // Get Year
        var type = GetType();
        var assemblyName = type.Assembly.GetName().Name;
        if (!int.TryParse(assemblyName[^2..], out var year))
            throw new ArgumentException($"{assemblyName} must end in 2 digits that signify the year");
        year += 2000;

        // Get Day
        var className = type.Name;
        if (!int.TryParse(className[^2..], out var day))
            throw new ArgumentException($"{className} must end in 2 digits that signify the day");

        return (year, day);
    }
}