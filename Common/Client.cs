using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Common.Enums;

namespace Common;

internal static partial class Client
{
    private const string IDPath = "..\\..\\..\\..\\SessionID";
    private const string SubmissionsPath = "Submissions";
    private const string InputsPath = "Inputs";

    private const string Host = "adventofcode.com";
    private const int ConsoleBufferSize = 4096;
    private const int SessionIDLength = 128;

    private const string AnswerCorrect = "That's the right answer!";
    private const string AnswerTooLow = "That's not the right answer; your answer is too low.";
    private const string AnswerTooHigh = "That's not the right answer; your answer is too high.";
    private const string AnswerIncorrect = "That's not the right answer.";
    private const string AnswerTooRecent = "You gave an answer too recently; ";
    private const string AnswerWrongLevel = "You don't seem to be solving the right level.";

    private static string _sessionID;
    private static string _sessionPath;
    private static CookieContainer _cookies;

    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private static Dictionary<ID, Dictionary<string, SubmissionResponse>> _submissionCache;
    private static Dictionary<ID, (string, string)> _submissionBounds;

    public static bool HasID => !string.IsNullOrWhiteSpace(_sessionID);

    /// <summary>
    /// Initialize the client
    /// </summary>
    public static void Initialize()
    {   // Set console buffer size
        var inputBuffer = new byte[ConsoleBufferSize];
        var inputStream = Console.OpenStandardInput(inputBuffer.Length);
        Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length));

        // Read Session ID
        _sessionPath = Path.GetFullPath(IDPath);
        if (!Path.Exists(_sessionPath) && !TryGetSessionID(_sessionPath)) return;
        ReadFile(_sessionPath, out var lines);
        _sessionID = lines[0];

        // Initilize submission cache(s)
        _submissionCache = [];
        _submissionBounds = [];
    }

    /// <summary>
    /// Get the input for the given day
    /// </summary>
    /// <param name="year">The year the task belongs to</param>
    /// <param name="day">The day the task belongs to</param>
    /// <param name="input">The input that was found</param>
    /// <returns>Whether or not input was found</returns>
    public static bool TryGetInput(int year, int day, out string[] input)
    {   // Allow user to input custom data
        IO.WriteLine("Enter custom input, or leave blank to use the real input:", Colors.Input);

        // Read the line
        var line = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(line))
            // Custom input, get the contents of the clipboard
            return TryGetCustomInput(line, out input);

        // Read official puzzle input
        Console.CursorTop -= 2;
        IO.ClearLine();

        return TryGetPuzzleInput(year, day, out input);
    }

    /// <summary>
    /// Submit the given result as an answer to given task
    /// </summary>
    /// <param name="year">The year the task belongs to</param>
    /// <param name="day">The day the task belongs to</param>
    /// <param name="part">The part of the task the result is for</param>
    /// <param name="answer">The answer to submit</param>
    public static void SubmitAnswer(int year, int day, int part, string answer)
    {   // Check if the response can be determined ahead of time
        Console.Write("Submitting ");
        IO.Write(answer, Colors.Positive);
        Console.WriteLine($" as the answer for part {part}");

        var id = new ID(year, day);
        var color = Colors.Negative;
        switch (SubmitAnswer(id, part, answer, out var message, out var cached))
        {   // Handle the response
            case SubmissionResponse.Correct:
                // Clear cached responses
                _submissionCache.Remove(id);
                _submissionBounds.Remove(id);

                // Delete cache file
                var cachePath = GetDayPath(SubmissionsPath, day);
                if (File.Exists(cachePath)) File.Delete(cachePath);

                message = AnswerCorrect;
                color = Colors.Positive;
                break;

            case SubmissionResponse.Incorrect:
                // Cache the response
                SaveResponse(id, answer, SubmissionResponse.Incorrect);
                message = AnswerIncorrect;
                break;

            case SubmissionResponse.TooLow:
                if (!cached)
                {   // Answer didn't hit the cache, lower bound might need to be updated
                    var (lower, upper) = GetBounds(id, _submissionCache[id]);
                    if (string.IsNullOrWhiteSpace(lower) || answer.CompareTo(lower) > 0)
                        _submissionBounds[id] = (answer, upper);
                }

                // Cache the response
                SaveResponse(id, answer, SubmissionResponse.TooLow);
                message = AnswerTooLow;
                break;

            case SubmissionResponse.TooHigh:
                if (!cached)
                {   // Answer didn't hit the cache, upper bound might need to be updated
                    var (lower, upper) = GetBounds(id, _submissionCache[id]);
                    if (string.IsNullOrWhiteSpace(upper) || answer.CompareTo(upper) < 0)
                        _submissionBounds[id] = (lower, answer);
                }

                // Cache the response
                SaveResponse(id, answer, SubmissionResponse.TooHigh);
                message = AnswerTooHigh;
                break;

            case SubmissionResponse.TooRecent:
                var segments = TooRecentRegex().Match(message).Groups;

                // Print the initial time
                Console.CursorVisible = false;
                IO.Write(AnswerTooRecent, Colors.TooRecent);
                IO.Write(segments[1].Value, Colors.TooRecent);

                var timerStart = Console.CursorLeft;

                // Print the time in a different color
                if (segments[2].Success) IO.Write(segments[2].Value, Colors.Neutral);   // Minutes
                if (segments[3].Success) IO.Write(segments[3].Value, Colors.Neutral);   // Seconds
                else Console.CursorLeft += 4;   // Seconds will be added once minutes tick down

                var timerEnd = Console.CursorLeft - 1;

                IO.Write(segments[4].Value, Colors.TooRecent);

                // Wait for time to elapse, then resubmit
                AwaitTimer(segments[2].Value, segments[3].Value, timerStart, timerEnd);
                SubmitAnswer(year, day, part, answer);
                Console.CursorVisible = true;
                return;

            case SubmissionResponse.WrongLevel:
                color = Colors.WrongLevel;
                break;

            case SubmissionResponse.Unknown when string.IsNullOrWhiteSpace(message):
                message = $"No response. Might need to refresh {_sessionPath}";
                break;

            case SubmissionResponse.Unknown:
                message = $"Unknown response: {message}";
                break;
        }

        // Print the message to the console
        IO.Write(message, color);

        if (cached)
            // Replace . with * to indicate that the response was predetermined
            IO.Write("*", Colors.Neutral);

        Console.WriteLine();

        // Allow user to see the submission result
        Console.ReadKey();
    }

    /// <summary>
    /// Submit the answer
    /// </summary>
    /// <param name="id">The ID of the task</param>
    /// <param name="part">The part of the task the result is for</param>
    /// <param name="answer">The answer to submit</param>
    /// <param name="message">The message to print</param>
    /// <param name="cached">Whether or not the response was from the cache</param>
    /// <returns>The response to the submission</returns>
    private static SubmissionResponse SubmitAnswer(ID id, int part, string answer, out string message, out bool cached)
    {   // Attempt to determine the response without querying the server
        if (cached = TryGetResponse(id, answer, out var submissionResponse))
        {
            message = submissionResponse switch
            {
                SubmissionResponse.TooLow => AnswerTooLow,
                SubmissionResponse.TooHigh => AnswerTooHigh,
                SubmissionResponse.Incorrect => AnswerIncorrect,
                _ => throw new NotImplementedException(),
            };

            return submissionResponse;
        }

        // Response could not be pre-determined, query the server
        var data = new[]
        {   // Set up POST request data
            new KeyValuePair<string, string>("level", part.ToString()),
            new KeyValuePair<string, string>("answer", answer),
        };

        var encodedContent = new FormUrlEncodedContent(data);

        // Send POST request
        var request = Request(id.Year, id.Day, c => c.PostAsync("answer", encodedContent));
        var response = request.Content.ReadAsStringAsync().Result;
        var extracted = ResponseRegex().Match(response);
        message = extracted.Groups[1].Value;

        // Use message to determine response
        if (message.StartsWith(AnswerCorrect)) return SubmissionResponse.Correct;
        if (message.StartsWith(AnswerTooLow)) return SubmissionResponse.TooLow;
        if (message.StartsWith(AnswerTooHigh)) return SubmissionResponse.TooHigh;
        if (message.StartsWith(AnswerIncorrect)) return SubmissionResponse.Incorrect;
        if (message.StartsWith(AnswerTooRecent)) return SubmissionResponse.TooRecent;
        if (message.StartsWith(AnswerWrongLevel)) return SubmissionResponse.WrongLevel;
        return SubmissionResponse.Unknown;
    }

    /// <summary>
    /// Reads custom user input.
    /// </summary>
    /// <param name="line">The line that was already read</param>
    /// <param name="input">The input that was read</param>
    /// <returns>Whether or not input was found</returns>
    private static bool TryGetCustomInput(string line, out string[] input)
    {   // Move input one line up
        Console.CursorTop -= 2;
        Console.Write(line);
        IO.ClearLine(line.Length);
        Console.WriteLine();
        IO.ClearLine();

        // If the line and clipboard contents match, the input was pasted
        var clipboard = IO.ReadClipboard();
        if (clipboard[0].Equals(line))
        {   // Consume all inputs, to avoid issues
            while (Console.KeyAvailable) Console.ReadKey(true);

            // Print the remaining content of the clipboard
            for (var i = 1; i < clipboard.Length; i++)
                Console.WriteLine(clipboard[i]);
            Console.WriteLine();

            input = clipboard;
            return true;
        }

        // Read lines until line is empty
        var lines = new List<string> { line }; // Include the already read line
        while ((line = Console.ReadLine()) != null && !string.IsNullOrWhiteSpace(line))
            lines.Add(line);

        input = [.. lines];
        return true;
    }

    /// <summary>
    /// Read the input file, if present. Otherwise, attempt to download it from the website
    /// </summary>
    /// <param name="year">The year the task belongs to</param>
    /// <param name="day">The day the task belongs to</param>
    /// <param name="input">The input found</param>
    /// <returns>Whether or not the input was found</returns>
    private static bool TryGetPuzzleInput(int year, int day, out string[] input)
    {   // Construct input path
        var localPath = GetDayPath(InputsPath, day);
        var fullPath = Path.GetFullPath(localPath);

        if (File.Exists(fullPath))
            // Input file found, read it
            return ReadFile(localPath, out input);

        $"Could not find {localPath}".WriteLine(Colors.Negative);

        if (HasID)
        {   // Send request to host
            "Downloading...".WriteLine(Colors.Neutral);

            try
            {
                var response = Request(year, day, c => c.GetStringAsync("input"));

                // Write response to file
                WriteFile(fullPath, response);
                "Download Complete!".WriteLine(Colors.Positive);
                Console.WriteLine();

                return ReadFile(fullPath, out input);
            }
            catch (AggregateException ae)
            {   // Host returns 404 when task is not (yet) available
                if (ae.InnerException is not HttpRequestException re) throw;
                if (re.StatusCode != HttpStatusCode.NotFound) throw;

                // Throw an exception with a more informative message
                throw new Exception($"{GetDayURL(year, day)} could not be found");
            }
        }

        // Puzzle input could not be found
        input = [];
        return false;
    }

    /// <summary>
    /// Provide instructions for user to find their session ID
    /// </summary>
    /// <param name="path">The path to the session ID file</param>
    /// <returns>Whether or not a session ID was provided</returns>
    private static bool TryGetSessionID(string path)
    {
        IO.WriteError($"Could not find session ID", false);
        Console.WriteLine();

        // Instructions how to find Session ID
        IO.WriteLine($"1. Log in to {Host}", Colors.Neutral);
        IO.WriteLine("2. Open the console (F12), and select 'Network'", Colors.Neutral);
        IO.WriteLine("3. Refresh the page", Colors.Neutral);
        IO.WriteLine($"4. Select an entry from the domain '{Host}'", Colors.Neutral);
        IO.WriteLine("5. Copy contents of the 'session' cookie", Colors.Neutral);
        IO.WriteLine("6. Paste the contents here, then press enter", Colors.Neutral);
        IO.Write("> ", Colors.Input);

        // The position of the input line
        var (left, top) = Console.GetCursorPosition();

        // Read user input
        Console.ForegroundColor = Colors.Input;
        var input = Console.ReadLine();
        var cookie = SessionCookieRegex().Match(input);
        var sessionID = cookie.Groups[2].Value;

        // Reset position of the cursor
        Console.CursorTop = top;
        Console.ForegroundColor = Colors.Neutral;

        if (string.IsNullOrWhiteSpace(sessionID))
        {   // No session ID provided
            IO.WriteError("No valid session ID provided, automatic features disabled", false);
            Console.WriteLine();
            Console.CursorTop++;
            return false;
        }

        // Clear Session ID text
        IO.ClearLine(left, input.Length);
        Console.CursorTop = top;
        IO.Write("Session ID provided!", Colors.Positive);

        if (sessionID.Length != SessionIDLength)
            // Warn that the session ID is not the expected length
            IO.WriteError($" ID length is not {SessionIDLength}; might not work properly", false);

        // Create session ID file
        File.WriteAllText(path, sessionID);
        Console.WriteLine();
        Console.CursorTop++;
        return true;
    }

    /// <summary>
    /// Attempt to pre-emptively determine the response from the server
    /// </summary>
    /// <param name="id">The ID of the task</param>
    /// <param name="answer">The submitted answer</param>
    /// <param name="response">The known response</param>
    /// <returns>Whether or not it was possible to determine the response</returns>
    private static bool TryGetResponse(ID id, string answer, out SubmissionResponse response)
    {
        var submissions = GetSubmissions(id);
        if (submissions.TryGetValue(answer, out response))
            // Previously submitted answer, response is already known
            return true;

        var (lower, upper) = GetBounds(id, submissions);
        if (!string.IsNullOrWhiteSpace(lower) && answer.CompareTo(lower) < 0)
        {   // The answer if less than the known lower bound
            SaveResponse(id, answer, response = SubmissionResponse.TooLow);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(upper) && answer.CompareTo(upper) > 0)
        {   // The answer is more than the known upper bound
            SaveResponse(id, answer, response = SubmissionResponse.TooHigh);
            return true;
        }

        response = SubmissionResponse.Unknown;
        return false;
    }

    /// <summary>
    /// Get all previously submitted results, and their responses
    /// </summary>
    /// <param name="id">The ID of the task</param>
    /// <returns>All submitted results, and their responses</returns>
    private static Dictionary<string, SubmissionResponse> GetSubmissions(ID id)
    {
        lock (_submissionCache)
        {
            if (_submissionCache.TryGetValue(id, out var cache))
                return cache;

            // Nothing cached, attempt to read from disc
            var path = GetDayPath(SubmissionsPath, id.Day);

            if (!Path.Exists(path))
                // Could not find any previous submissions
                return _submissionCache[id] = [];

            ReadFile(path, out var lines);
            var contents = string.Join(Environment.NewLine, lines);
            return _submissionCache[id] = JsonSerializer.Deserialize<Dictionary<string, SubmissionResponse>>(contents);
        }
    }

    /// <summary>
    /// Get the response bounds, based on known submission responses
    /// </summary>
    /// <param name="id">The ID of the task</param>
    /// <param name="submissions">Known submission responses</param>
    /// <returns>The known bounds for responses</returns>
    private static (string, string) GetBounds(ID id, Dictionary<string, SubmissionResponse> submissions)
    {
        lock (_submissionBounds)
        {
            if (_submissionBounds.TryGetValue(id, out var bounds))
                return bounds;

            // Check what the highest seen "Too Low" submission is, if any
            string lower = null;
            var tooLow = submissions.Where(s => s.Value == SubmissionResponse.TooLow);
            if (tooLow.Any()) lower = tooLow.Max(s => s.Key);

            // Check what the lowest seen "Too High" submission is, if any
            string upper = null;
            var tooHigh = submissions.Where(s => s.Value == SubmissionResponse.TooHigh);
            if (tooHigh.Any()) upper = tooHigh.Min(s => s.Key);

            return _submissionBounds[id] = (lower, upper);
        }
    }

    /// <summary>
    /// Tick down the timer until it has expired
    /// </summary>
    /// <param name="minutes">The minutes to wait</param>
    /// <param name="seconds">The seconds to wait</param>
    /// <param name="timerStart">The location where the timer starts</param>
    /// <param name="timerEnd">The location where the timer ends</param>
    private static void AwaitTimer(string minutes, string seconds, int timerStart, int timerEnd)
    {   // Calculate the time that submission is allowed
        var targetTime = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(minutes))
        {   // Append minutes to target time
            minutes = minutes[..^2];
            var min = int.Parse(minutes);
            targetTime = targetTime.AddMinutes(min);
        }

        if (!string.IsNullOrWhiteSpace(seconds))
        {   // Append seconds to target time
            seconds = seconds[..^2];
            var sec = int.Parse(seconds);
            targetTime = targetTime.AddSeconds(sec);
        }

        // Set up update timer
        var printer = new StringBuilder();
        var printedWidth = timerEnd - timerStart;
        var timer = new System.Timers.Timer(500) { AutoReset = true };
        timer.Elapsed += (o, e) =>
        {   // Calculate remaining time
            var remaining = targetTime - DateTime.Now;
            printer.Clear();

            lock (timer)
            {   // Exit out if the time has elapsed
                timer.Enabled &= remaining.TotalSeconds > 0;
                if (!timer.Enabled) return;

                if (remaining.Minutes > 0)
                {   // Append minutes to text
                    var min = remaining.Minutes.ToString();
                    printer.Append(min.PadLeft(minutes.Length));
                    printer.Append("m ");
                }

                if (remaining.Minutes > 0 && remaining.Seconds < 10)
                {   // Prepend a 0 to seconds if there are still minutes remaining
                    printer.Append('0');
                }

                // Append seconds to text
                printer.Append(remaining.Seconds);
                printer.Append('s');

                // The width of the printed time should remain consistent
                while (printer.Length < printedWidth)
                    printer.Insert(0, ' ');

                // Print the remaining time
                Console.CursorLeft = timerStart;
                IO.Write(printer.ToString(), Colors.Neutral);
            }
        };

        // Run the timer until time has run out
        timer.Enabled = true;
        while (timer.Enabled) { }

        lock (timer)
        {   // Clear the line, and move back to the original line
            IO.ClearLine();
            Console.CursorTop--;
        }
    }

    /// <summary>
    /// Save the response to file
    /// </summary>
    /// <param name="id">The ID of the task</param>
    /// <param name="answer">The submitted answer</param>
    /// <param name="response">The result to the answer</param>
    private static void SaveResponse(ID id, string answer, SubmissionResponse response)
    {
        _submissionCache[id][answer] = response;
        var path = GetDayPath(SubmissionsPath, id.Day);
        var contents = JsonSerializer.Serialize(_submissionCache[id], _options);
        WriteFile(path, contents);
    }

    /// <summary>
    /// Send an HTTP request to the server
    /// </summary>
    /// <param name="year">The year that the request relates to</param>
    /// <param name="day">The day that the request relates to</param>
    /// <param name="method">The command to run on the <see cref="HttpClient"/></param>
    /// <returns>The response from the server</returns>
    private static T Request<T>(int year, int day, Func<HttpClient, Task<T>> method)
    {
        if (_cookies == null)
        {   // Create cookie(s)
            var cookie = new Cookie("session", _sessionID, "/", Host);
            _cookies = new CookieContainer();
            _cookies.Add(cookie);
        }

        using var handler = new HttpClientHandler
        {
            CookieContainer = _cookies,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

        var baseURL = GetDayURL(year, day);
        using var client = new HttpClient(handler) { BaseAddress = new Uri(baseURL) };
        return method(client).Result;
    }

    /// <summary>
    /// Get the path to the file for given day
    /// </summary>
    /// <param name="root">The root location of the path</param>
    /// <param name="day">The day whose file to get</param>
    /// <returns>The path to the file</returns>
    private static string GetDayPath(string root, int day) =>
        $"{root}/{day:00}.txt";

    /// <summary>
    /// Get the URL to the task for given day
    /// </summary>
    /// <param name="year">The year the task belongs to</param>
    /// <param name="day">The day the task belongs to</param>
    /// <returns>The URL to the task</returns>
    private static string GetDayURL(int year, int day) =>
        $"https://{Host}/{year}/day/{day}/";

    /// <summary>
    /// Read the contents of file found at given path
    /// </summary>
    /// <param name="path">The path to the file</param>
    /// <param name="contents">The contents of the file</param>
    /// <returns>Whether or not the read was successful</returns>
    public static bool ReadFile(string path, out string[] contents)
    {
        try
        {
            using var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var readStream = new StreamReader(fileStream);

            string line;
            var lines = new List<string>();
            while ((line = readStream.ReadLine()) != null)
                lines.Add(line);

            contents = [.. lines];
            return true;
        }
        catch (Exception e)
        {
            IO.WriteError($"Could not open '{path}'", false);
            IO.WriteError(e.Message);

            contents = [];
            return false;
        }
    }

    /// <summary>
    /// Write data to file
    /// </summary>
    /// <param name="path">The path where the file should be written</param>
    /// <param name="contents">The contents of the file to be written</param>
    public static void WriteFile(string path, string contents)
    {
        var directory = Path.GetDirectoryName(path);
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, contents);
    }

    private record ID(int Year, int Day);

    [GeneratedRegex("(session=)?([A-Za-z0-9]*)")]
    private static partial Regex SessionCookieRegex();

    [GeneratedRegex("<article><p>((.|\n)*?)<a href=")]
    private static partial Regex ResponseRegex();

    [GeneratedRegex("(You have )([0-9]*?m )?([0-9]{0,2}s )?(left.*)")]
    private static partial Regex TooRecentRegex();
}