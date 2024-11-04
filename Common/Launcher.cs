using System.Reflection;
using System.Runtime.CompilerServices;

namespace Common;

public static class Launcher
{   /// <summary>
    /// Run today's task
    /// </summary>
    public static void Run([CallerFilePath] string callPath = null) =>
        Run(DateTime.Now.Day, callPath);

    /// <summary>
    /// Run task for the given day
    /// </summary>
    /// <param name="day">The day whose task to run</param>
    public static void Run(int day, [CallerFilePath] string callPath = null)
    {   // Ensure parameter is a valid date
        day = Math.Max(day, 1);
        day = Math.Min(day, 25);

        // Initialize the client
        Client.Initialize();

        // Determine class type
        var assembly = Assembly.GetEntryAssembly();
        var assemblyName = assembly.GetName().Name;
        var fullName = $"{assemblyName}.Day{day:00}";
        var classType = assembly.GetType(fullName);
        if (classType == null)
        {
            IO.WriteError($"Could not find the class {fullName}", false);
            var target = $"{assemblyName}.{Path.GetFileNameWithoutExtension(callPath)}.{nameof(Run)}";
            IO.WriteError($"You can run a different day by calling {target} with a different parameter");
            return;
        }

        // Validate class type
        if (!typeof(DayBase).IsAssignableFrom(classType))
        {
            IO.WriteError($"{fullName} does not inherit from {nameof(DayBase)}");
            return;
        }

        try
        {   // Attempt to create an instance of class type
            var instance = Activator.CreateInstance(classType) as DayBase;
            instance.Run();
        }
        catch (Exception ex)
        {   // Something went wrong
            var message = ex.InnerException?.Message ?? ex.Message;
            IO.WriteError(message, false);
            IO.WriteError(ex.StackTrace);
        }
    }
}