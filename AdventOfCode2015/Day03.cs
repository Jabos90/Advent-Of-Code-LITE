using Common;
using Common.Enums;
using Common.Extensions;

namespace AdventOfCode2015;

internal class Day03 : DayBase
{
    protected override void Run(string[] input)
    {
        var solo = (0, 0);
        var soloDeliveries = new Dictionary<int, Dictionary<int, int>>();

        var coopSanta = (0, 0);
        var coopRobot = (0, 0);
        var coopDeliveries = new Dictionary<int, Dictionary<int, int>>();

        // Place a present in the first house
        soloDeliveries.Add(solo, 1);
        coopDeliveries.Add(coopSanta, 1);

        var oddDelivery = true;
        foreach(var move in input[0])
        {   // Decipher the indicated direction
            Cardinal direction = move switch
            {
                '^' => direction = Cardinal.North,
                '>' => direction = Cardinal.East,
                'v' => direction = Cardinal.South,
                '<' => direction = Cardinal.West,
                _ => throw new ArgumentException("Unknown direction"),
            };

            // Move santa
            solo.Move(direction);
            soloDeliveries.Add(solo, 1);

            if (oddDelivery)
            {   // Santa listens to odd-numbered instructions
                coopSanta.Move(direction);
                coopDeliveries.Add(coopSanta, 1);
            }
            else
            {   // Robot listens to even-numbered instructions
                coopRobot.Move(direction);
                coopDeliveries.Add(coopRobot, 1);
            }

            oddDelivery = !oddDelivery;
        }

        PrintResult("Santa delivers presents to", soloDeliveries.Count(), "houses");
        PrintResult("With the help of a Robot", coopDeliveries.Count(), "houses receive presents");
    }
}