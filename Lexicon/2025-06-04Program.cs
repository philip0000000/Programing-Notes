

namespace CSharpConditionalStatementExercises
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ConditionalStatementExercises.TwoIntEqual();
            //ConditionalStatementExercises.NumberEvenOrOdd();
            //ConditionalStatementExercises.LargestNumber();
            //ConditionalStatementExercises.VowelOrConsonant();
        }
    }

    public class ConditionalStatementExercises
    {
        public static void TwoIntEqual()
        {
            int num1, num2;

            Console.Write("Input 1st number: ");
            num1 = Convert.ToInt32(Console.ReadLine());
            Console.Write("Input 2nd number: ");
            num2 = Convert.ToInt32(Console.ReadLine());

            if (num1 == num2)
            {
                Console.WriteLine("The two numbers are equal.");
            }
            else
            {
                Console.WriteLine("The two numbers are not equal.");
            }
        }

        public static void NumberEvenOrOdd()
        {
            int num;

            Console.Write("Input a number: ");
            num = Convert.ToInt32(Console.ReadLine());

            if (num % 2 == 0)
            {
                Console.WriteLine("The number is even.");
            }
            else
            {
                Console.WriteLine("The number is odd.");
            }
        }

        public static void LargestNumber()
        {
            List<int> numbers = new List<int>();

            Console.WriteLine("This program takes any amount of numbers and returns a list " +
                "from largest to smallest. To end input, enter no new number (blank input).");

            while (true)
            {
                Console.Write("Input a number: ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    // Exit loop on blank input
                    break;
                }

                if (int.TryParse(input, out int number))
                {
                    numbers.Add(number);
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }

            // Sort descending (high to low)
            numbers.Sort((a, b) => b.CompareTo(a));

            Console.WriteLine("The numbers from largest to smallest are:");
            foreach (var number in numbers)
            {
                Console.WriteLine(number);
            }
        }

        public static void VowelOrConsonant()
        {
            Console.Write("Input a letter: ");
            char letter = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if ("aeiouAEIOU".IndexOf(letter) >= 0)
            {
                Console.WriteLine($"{letter} is a vowel.");
            }
            else if (char.IsLetter(letter))
            {
                Console.WriteLine($"{letter} is a consonant.");
            }
            else
            {
                Console.WriteLine($"{letter} is not a valid letter.");
            }
        }
    }

    // Number is monster level, can have 2 attacks, walk in 4 rooms, defeat enemies to spawn new monsters, 1 randome point exist on map that increase level

    public class SmallGame
    {
        // Map
        private readonly int _width;
        private readonly int _height;
        private readonly char[,] _map;

        //TODO: cordinates

        // Game objects
        // Player
        //...
        //Monsters
        //...

        // Constructor to initialize the game map with the given dimensions
        public SmallGame(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

            _width = width;
            _height = height;
            _map = new char[_height, _width];

            InitializeMap();
        }

        // Initialize the map to fill with '.'
        private void InitializeMap()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _map[y, x] = '.';
                }
            }
        }

        // Utility to print the map to the console (for testing/debugging)
        private void PrintMap()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    Console.Write(_map[y, x]);
                }
                Console.WriteLine();
            }
        }

        public void GameLoop()
        {
            PrintMap();
        }
        // Optionally, expose methods to manipulate the map, e.g., SetTile(x, y, tile)
    }
}
