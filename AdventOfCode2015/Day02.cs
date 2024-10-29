using Common;
using Common.Extensions;

namespace AdventOfCode2015;

internal class Day02 : DayBase
{
    protected override void Run(string[] input)
    {
        var requiredWrappingPaper = 0;
        var requiredLengthOfRibbon = 0;
        foreach (var present in input)
        {
            var (length, width, height, _) = present.Split('x');
            var sides = new List<int> { length.ToInt(), width.ToInt(), height.ToInt() };

            var area1 = sides[0] * sides[1];
            var area2 = sides[1] * sides[2];
            var area3 = sides[2] * sides[0];

            sides.Sort();

            requiredWrappingPaper += 2 * (area1 + area2 + area3) + Sequences.Min(area1, area2, area3);
            requiredLengthOfRibbon += 2 * (sides[0] + sides[1]) + sides[0] * sides[1] * sides[2];
        }

        PrintResult("The elves require", requiredWrappingPaper, "feet of wrapping paper");
        PrintResult("The elves require", requiredLengthOfRibbon, "feet of ribbon");
    }
}