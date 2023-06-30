//
// PER12480I-2587-Exempelproblem-Graz-Sweden-AB
// Author: philip0000000
// Version: 1.1
// Date: 2023-06-28
//

// Fixed the line:
//      BigInteger temp = (stackedGlasses[r][g].Denominator - stackedGlasses[r][g].Numerator) / addTable[r][g];
// and the line:
//      if (r == rad - 1 && g == glas - 1 && temp * addTable[r][g] == stackedGlasses[r][g].Denominator)
//          timesUntilFull--;


#undef DEBUG


using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;


public class Fraction
{
    public BigInteger Numerator { get; set; }
    public BigInteger Denominator { get; private set; }

    public Fraction(BigInteger numerator, BigInteger denominator)
    {
        if (denominator == 0)
            throw new ArgumentException("Nämnare kan inte vara noll.");

        Numerator = numerator;
        Denominator = denominator;
    }

    public static Fraction operator +(Fraction a, Fraction b)
    {
        BigInteger commonDenominator = a.Denominator * b.Denominator;
        BigInteger commonNumerator = a.Numerator * b.Denominator + b.Numerator * a.Denominator;
        return new Fraction(commonNumerator, commonDenominator);
    }

    public void Simplify()
    {
        BigInteger gcd = GCD(Numerator, Denominator);
        Numerator /= gcd;
        Denominator /= gcd;
    }

    private static BigInteger GCD(BigInteger a, BigInteger b)
    {
        while (b != 0)
        {
            BigInteger temp = b;
            b = a % b;
            a = temp;
        }
        return BigInteger.Abs(a);
    }

    public override string ToString()
    {
        return $"{Numerator}/{Denominator}";
    }
}

class Program
{
    static int GetUserInput(string prompt, int min, int max, string errorMsg)
    {
        int ret;
        do
        {
            Console.WriteLine(prompt);
            if (!int.TryParse(Console.ReadLine(), out ret) || ret < min || ret > max)
                Console.WriteLine(errorMsg);
        } while (ret < min || ret > max);

        return ret;
    }

    static void PrintGlassPyramid1(List<Fraction[]> stackedGlasses)
    {
        Console.WriteLine();
        int pyramidHeight = stackedGlasses.Count;
        var sb = new StringBuilder();

        for (int i = 0; i < pyramidHeight; i++)
        {
            // Print leading spaces
            for (int j = 0; j < pyramidHeight - i - 1; j++)
                sb.Append("       ");
            // Print glasses
            for (int k = 0; k < stackedGlasses[i].Length; k++)
                sb.Append(stackedGlasses[i][k].ToString() + "       ");
            // Move to the next line
            sb.AppendLine();
        }
        Console.Write(sb.ToString());
    }
    static void PrintGlassPyramid2(List<BigInteger[]> stackedGlasses)
    {
        Console.WriteLine();
        int pyramidHeight = stackedGlasses.Count;
        var sb = new StringBuilder();

        for (int i = 0; i < pyramidHeight; i++)
        {
            // Print leading spaces
            for (int j = 0; j < pyramidHeight - i - 1; j++)
                sb.Append("     ");
            // Print glasses
            for (int k = 0; k < stackedGlasses[i].Length; k++)
                sb.Append(stackedGlasses[i][k] + "     ");
            // Move to the next line
            sb.AppendLine();
        }
        Console.Write(sb.ToString());
    }

    public static char FindMostCommonDigit(string numString)
    {
        Dictionary<char, int> digitCounts = new Dictionary<char, int>();

        foreach (char c in numString)
            if (digitCounts.ContainsKey(c))
                digitCounts[c]++;
            else
                digitCounts[c] = 1;

        char mostCommonDigit = numString[0];
        int highestCount = digitCounts[mostCommonDigit];

        foreach (KeyValuePair<char, int> pair in digitCounts)
            if (pair.Value > highestCount)
            {
                mostCommonDigit = pair.Key;
                highestCount = pair.Value;
            }

        return mostCommonDigit;
    }

    /// <summary>
    /// Calculate the duration it takes for a specific glass in a pyramid formed by stacking glassware to begin overflowing.
    /// </summary>
    /// <param name="rad">The row in which the glass is located.</param>
    /// <param name="glas">The specific glass within the row to monitor.</param>
    /// <returns>The duration in seconds.</returns>
    static string CalculationStackedGlasses(int rad, int glas)
    {
        // --- Initialize variables ---
        // Determine accuracy of calculation
        BigInteger WATER_TO_ADD = BigInteger.Parse("1");
        BigInteger PRECISION = BigInteger.Parse("10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");

        // Create stacked glasses data
        var stackedGlasses = new List<Fraction[]>
        {
            new Fraction[1] { new Fraction(0, PRECISION) } // Each glass is represented as a fraction for most accurate results
        };
        for (int r = 1; r <= rad; r++) // Add rows of glasses to "rad" and then add one more glass for overflow(floor)
        {
            stackedGlasses.Add(new Fraction[stackedGlasses.Count + 1]);
            for (int g = 0; g < stackedGlasses.Count; g++)
                stackedGlasses[stackedGlasses.Count - 1][g] = new Fraction(0, stackedGlasses[stackedGlasses.Count - 2][0].Denominator * 2); // Double the denominator for each row
        }

        // For optimisation
        var addTable = new List<BigInteger[]>(); // Table to show the current rate at which a glass is being filled
        for (int r = 1; r <= rad + 1; r++)
            addTable.Add(new BigInteger[r]);
        bool endOptimisation = false;

        // --- Simulation ---
        bool endLoop = false;
        while (!endLoop) // Run loop until the specific glass has been filled
        {
            // (Reset the addition table. For optimization)
            for (int r = 0; r < addTable.Count; r++)
                for (int g = 0; g < addTable[r].Length; g++)
                    addTable[r][g] = 0;

            // Add water to the top glass of the pyramid 
            stackedGlasses[0][0].Numerator += WATER_TO_ADD;
            addTable[0][0] += WATER_TO_ADD;                    // (For optimisation)

            // Trickle down the water if the glass is more than full, indicated by the numerator value corresponding to the denominator
            for (int r = 0; r < stackedGlasses.Count - 1; r++) // Ignore the last row, to accumulate water on the floor
            {
                for (int g = 0; g < stackedGlasses[r].Length; g++)
                {
                    BigInteger tempDenominator = stackedGlasses[r][0].Denominator;
                    if (stackedGlasses[r][g].Numerator > tempDenominator)                     // Glass has too much water, trickle down water to glass below
                    {
                        BigInteger overflowWater = (stackedGlasses[r][g].Numerator - tempDenominator); // / 2) * 2; // No need to divide the water because the denominator of the rows below is double in size
                        stackedGlasses[r + 1][g].Numerator += overflowWater;
                        stackedGlasses[r + 1][g + 1].Numerator += overflowWater;
                        // (Update add table. For optimisation)
                        addTable[r + 1][g] += addTable[r][g];
                        addTable[r + 1][g + 1] += addTable[r][g];
                        // Make the glass full as excess water has been poured into the glass below
                        stackedGlasses[r][g].Numerator = tempDenominator;
                    }
                }

                // Terminate the loop if the current glass, determined by rad and glas, is full
                if (stackedGlasses[rad - 1][glas - 1].Numerator >= stackedGlasses[rad - 1][glas - 1].Denominator)
                {
                    endLoop = true;
                    break;
                }
            }
            if (endLoop == true)
                break;

            // Debugging
#if DEBUG
            PrintGlassPyramid1(stackedGlasses);
            PrintGlassPyramid2(addTable);
#endif

            // --- Optimisation ---
            if (endOptimisation == true)
                continue;

            // Find the glass that will fastest fill up in current state of the glass pyramid
            // and fill all glasses in the pyramid of that number
            BigInteger timesUntilFull = BigInteger.Parse("0"); // Lowest amount of times, the next glass that will be full

            // Identify the glass in the current state of the glass pyramid that will fill up the most rapidly
            for (int r = 0; r < stackedGlasses.Count - 1; r++) // Ignore last row(floor)
                for (int g = 0; g < stackedGlasses[r].Length; g++)
                    if (stackedGlasses[r][g].Numerator > 0 && stackedGlasses[r][g].Numerator < stackedGlasses[r][g].Denominator) // Glass is not empty or full
                    {
                        // Calculate how many steps until glass will be full
                        BigInteger temp = (stackedGlasses[r][g].Denominator - stackedGlasses[r][g].Numerator) / addTable[r][g];

                        if ((temp < timesUntilFull && temp > 0) || // Save lowest value we find
                            timesUntilFull == 0)                   // Initialize value, if no value exist. Add the first value we find.
                        {
                            timesUntilFull = temp;

                            // If the glass being monitored is expected to reach full capacity through the optimization operation, reduce the number of water pouring by two or three
                            // We want to do the last two or three pouring of water in the simulation step
                            if (r == rad - 1 &&
                               (g == glas - 1 || g == stackedGlasses[r].Length - glas)) // Row is mirrowed, left and right side will have same value. Account for that.
                            {
                                timesUntilFull -= 3;
                                if (timesUntilFull < 0)
                                {
                                    timesUntilFull = 0;
                                    endOptimisation = true; // End optimisation
                                }
                            }
                        }
                    }

            if (endOptimisation == true)
                continue;

            // Add water to each glass
            for (int r = 0; r < stackedGlasses.Count; r++)
                for (int g = 0; g < stackedGlasses[r].Length; g++)
                    if ((stackedGlasses[r][g].Numerator > 0 && stackedGlasses[r][g].Numerator < stackedGlasses[r][g].Denominator) || // If glass is not full and not empty
                        r == stackedGlasses.Count - 1)                                                                               // If last row(floor)
                        stackedGlasses[r][g].Numerator += addTable[r][g] * timesUntilFull;

            // Debugging
#if DEBUG
            PrintGlassPyramid1(stackedGlasses);
            PrintGlassPyramid2(addTable);
#endif
        }

#if DEBUG
        PrintGlassPyramid1(stackedGlasses);
#endif

        // --- Calculate time ---
        double time = 0;
        Fraction fractionAccumulation = new Fraction(0, 1);

        for (int r = 0; r < stackedGlasses.Count; r++)
            for (int g = 0; g < stackedGlasses[r].Length; g++)
                if (stackedGlasses[r][g].Numerator >= stackedGlasses[r][g].Denominator &&   // A glass can only be full with max 10, if more discard what is left
                    r != stackedGlasses.Count - 1)                                          // Do not do this on the last row(floor), as on the floor it is accumulating
                    time += 10;
                else if (stackedGlasses[r][g].Numerator > 0)
                {
                    fractionAccumulation = fractionAccumulation + stackedGlasses[r][g];
                    fractionAccumulation.Simplify();
                }

        // --- Format value and return result ---
        // double
        double dTime = time + 10 * (double)fractionAccumulation.Numerator / (double)fractionAccumulation.Denominator;

        // Double can only represent exactly max 15-digit number.
        // Can maybe use dTime.ToString("R"), but best is to use BigInteger.DivRem and rounding up if needed.
        if (dTime <= 999999999999999f)
            return dTime.ToString("F3"); // Return the remainder as a floating-point number with a precision of 3 digits

        // string
        // The number is too big for double to be exactly represented, use BigInteger.DivRem and string
        var sb = new StringBuilder();

        BigInteger numerator = 10 * fractionAccumulation.Numerator;
        BigInteger quotient = BigInteger.DivRem(numerator, fractionAccumulation.Denominator, out BigInteger remainder);

        quotient += new BigInteger(time);

        // Rounding up if needed
        if (remainder.ToString().Length > 3 &&                // Only round up if more then 3 digits in remainder, for best precision
            FindMostCommonDigit(remainder.ToString()) == '9') // If number 9 is the most common digit in the remainder, rounding up
        {
            quotient += 1;
            sb.Append(quotient.ToString());
            sb.Append(",000");
        }
        else
        {
            sb.Append(quotient.ToString());
            sb.Append(",");
            sb.Append(remainder.ToString().Substring(0, Math.Min(3, remainder.ToString().Length))); // Only add 3 digits of remainder
            sb.Append(new string('0', Math.Max(0, 3 - remainder.ToString().Length)));               // Add 0 until 3 digit in remainder
        }

        return sb.ToString();
    }

    static void Main(string[] args)
    {
        try
        {
            // Get user input
            int rad = GetUserInput("Rad ? ", 2, 50, "Rad måste vara mellan 2 till 50!");
            int glas = GetUserInput("Glas ? ", 1, rad, "Måste vara mera än 0 och mindre än rad!");

            // Calculate
            string time = CalculationStackedGlasses(rad, glas);

            // Output result
            Console.WriteLine($"Det tar {time} sekunder.");
            Console.ReadLine();
        }
        catch (Exception e)
        {
            Console.WriteLine("Ett oväntat fel har inträffat: " + e.Message);
        }
    }
}


/*
Algorithm used: dynamic programing and simulation
Big O notation: O(n)

1                                                         10,000
2                                                 30,000       30,000
3                                         70,000       50,000       70,000
4                                 149,999       83,334       83,334       149,999
5                         301           136,667       110,000       136,667       301    
6                 630           220           154,000       154,000       220           630    
7         1270           366           218,000       186,000       218,000       366           1270    
8 2550           622           310           237,059       237,059       310           622           2550    
Rad9 Glas5 274,706
Rad10 Glas3 686,296
Rad10 Glas2 1812,857
Rad11 Glas7 414,445
Rad12 Glas4 780,181
Rad13 Glas4 1128,523
Rad49 Glas24 3795,614
Rad50 Glas25 3854,660

    --------
    Left most:
    2x+10

    In JavaScript:
var sum = 10;
for (var i = 1; i <= 50; i++) {
    console.log(sum + "\t\t\t\t\ti:" + (i));
    sum = (sum * 2) + 10;
}
    --------
*/




