using Common;

namespace AdventOfCode2015;

internal class Day01 : DayBase
{
    protected override void Run(string[] input)
    {
        var floor = 0;
        var instruction = 1;
        var basementEntered = false;
        foreach (var direction in input[0])
        {
            if (direction == '(')
                floor++;
            else if (direction == ')')
            {
                floor--;
                basementEntered |= floor == -1;
            }

            if (!basementEntered)
                instruction++;
        }

        PrintResult("The instructions take Santa to floor", floor);
        PrintResult("Instruction", instruction, "made Santa enter the basement");
    }
}