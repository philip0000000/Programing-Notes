using System;
using System.Collections.Generic;

namespace CSharpConditionalStatementExercises
{
    public struct Coordinate
    {
        public int X { get; }
        public int Y { get; }

        public Coordinate(int x, int y)
        {
            if (x < 0) throw new ArgumentOutOfRangeException(nameof(x), "X must be non-negative.");
            if (y < 0) throw new ArgumentOutOfRangeException(nameof(y), "Y must be non-negative.");
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(Coordinate a, Coordinate b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Coordinate a, Coordinate b) => !(a == b);
        public override bool Equals(object obj) => obj is Coordinate other && this == other;
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    public class Player
    {
        public Coordinate Position { get; private set; }
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; } = 0;
        private const int ExperienceToLevelUp = 3;

        public Player(Coordinate start)
        {
            Position = start;
        }

        public void MoveTo(Coordinate newPosition)
        {
            Position = newPosition;
        }

        public void GainExperience(int amount)
        {
            Experience += amount;
            if (Experience >= ExperienceToLevelUp)
            {
                Experience -= ExperienceToLevelUp;
                Level++;
                Console.WriteLine("You have leveled up! New level: " + Level);
            }
        }

        public string GetExperienceBar()
        {
            return $"EXP: {Experience}/{ExperienceToLevelUp}";
        }
    }

    public enum MonsterMovementType
    {
        Random,
        Vertical,
        Horizontal
    }

    public class Monster
    {
        public Coordinate Position { get; private set; }
        public int Level { get; private set; }
        public MonsterMovementType MovementType { get; }
        private bool _moveVertically; // For even-level monsters only

        public Monster(Coordinate position, int level)
        {
            if (level <= 0)
                throw new ArgumentOutOfRangeException(nameof(level), "Level must be positive.");
            Position = position;
            Level = level;

            if (Level % 2 == 0)
            {
                MovementType = new Random().Next(0, 2) == 0 ? MonsterMovementType.Vertical : MonsterMovementType.Horizontal;
            }
            else
            {
                MovementType = MonsterMovementType.Random;
            }
        }

        public void Move(Coordinate newPosition)
        {
            Position = newPosition;
        }

        public void MoveInDirection(int width, int height, Random random)
        {
            int direction = MovementType switch
            {
                MonsterMovementType.Vertical => random.Next(0, 2) == 0 ? 0 : 1, // up or down
                MonsterMovementType.Horizontal => random.Next(0, 2) == 0 ? 2 : 3, // left or right
                _ => random.Next(0, 4)
            };

            Coordinate newPosition = Position;
            switch (direction)
            {
                case 0: if (Position.Y > 0) newPosition = new Coordinate(Position.X, Position.Y - 1); break;
                case 1: if (Position.Y < height - 1) newPosition = new Coordinate(Position.X, Position.Y + 1); break;
                case 2: if (Position.X > 0) newPosition = new Coordinate(Position.X - 1, Position.Y); break;
                case 3: if (Position.X < width - 1) newPosition = new Coordinate(Position.X + 1, Position.Y); break;
            }
            Position = newPosition;
        }
    }

    public class AsciiGame
    {
        private readonly int _width;
        private readonly int _height;
        private readonly char[,] _map;
        private readonly Random _random = new();

        private Player _player;
        private int _playerMoveCounter = 0;
        private readonly List<Monster> _monsters = new();
        private Coordinate _experienceOrb;

        private readonly Dictionary<Monster, bool> _evenMonsterDirectionForward = new();

        private string _temporaryMessage = null;
        private int _temporaryMessageCounter = 0;

        public AsciiGame(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

            _width = width;
            _height = height;
            _map = new char[_height, _width];

            InitializeMap();
            InitializeGameObjects();
        }

        private void InitializeMap()
        {
            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                    _map[y, x] = '.';
        }

        private void InitializeGameObjects()
        {
            _player = new Player(new Coordinate(0, 0));
            _monsters.Clear();
            _evenMonsterDirectionForward.Clear();

            for (int i = 0; i < 9; i++)
            {
                int levelOffset = _random.Next(0, 3); // 0, 1, or 2
                int monsterLevel = _player.Level + levelOffset;

                Coordinate position;
                do
                {
                    int x = _random.Next(0, _width);
                    int y = _random.Next(0, _height);
                    position = new Coordinate(x, y);
                } while (position == _player.Position || _monsters.Exists(m => m.Position == position));

                var monster = new Monster(position, monsterLevel);
                _monsters.Add(monster);

                // Initialize direction as "forward" (true)
                if (monster.Level % 2 == 0)
                {
                    _evenMonsterDirectionForward[monster] = true;
                }
            }

            SpawnExperienceOrb();
        }

        private void SpawnExperienceOrb()
        {
            Coordinate position;
            do
            {
                int x = _random.Next(0, _width);
                int y = _random.Next(0, _height);
                position = new Coordinate(x, y);
            } while (position == _player.Position || _monsters.Exists(m => m.Position == position));

            _experienceOrb = position;
        }

        private void RenderObjects()
        {
            InitializeMap();

            _map[_player.Position.Y, _player.Position.X] = 'p';

            foreach (var monster in _monsters)
            {
                char display = (monster.Level >= 1 && monster.Level <= 9)
                    ? char.Parse(monster.Level.ToString())
                    : 'M';
                _map[monster.Position.Y, monster.Position.X] = display;
            }

            _map[_experienceOrb.Y, _experienceOrb.X] = 'e';
        }

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

            Console.WriteLine($"Player Level: {_player.Level}");
            Console.WriteLine(_player.GetExperienceBar());

            if (!string.IsNullOrEmpty(_temporaryMessage))
            {
                Console.WriteLine($"\n {_temporaryMessage}");
            }
        }

        private void MoveMonsters()
        {
            // Move even-level monsters
            foreach (var monster in _monsters.Where(m => m.Level % 2 == 0))
            {
                bool movingForward = _evenMonsterDirectionForward[monster];
                Coordinate oldPosition = monster.Position;
                Coordinate newPosition = monster.Position;

                if (monster.MovementType == MonsterMovementType.Vertical)
                {
                    int newY = monster.Position.Y + (movingForward ? 1 : -1);
                    if (newY < 0 || newY >= _height || _monsters.Any(m => m != monster && m.Position == new Coordinate(monster.Position.X, newY)))
                    {
                        // Reverse direction and move
                        _evenMonsterDirectionForward[monster] = !movingForward;
                        newY = monster.Position.Y + (_evenMonsterDirectionForward[monster] ? 1 : -1);
                        if (newY >= 0 && newY < _height && !_monsters.Any(m => m != monster && m.Position == new Coordinate(monster.Position.X, newY)))
                        {
                            newPosition = new Coordinate(monster.Position.X, newY);
                        }
                    }
                    else
                    {
                        newPosition = new Coordinate(monster.Position.X, newY);
                    }
                }
                else if (monster.MovementType == MonsterMovementType.Horizontal)
                {
                    int newX = monster.Position.X + (movingForward ? 1 : -1);
                    if (newX < 0 || newX >= _width || _monsters.Any(m => m != monster && m.Position == new Coordinate(newX, monster.Position.Y)))
                    {
                        // Reverse direction and move
                        _evenMonsterDirectionForward[monster] = !movingForward;
                        newX = monster.Position.X + (_evenMonsterDirectionForward[monster] ? 1 : -1);
                        if (newX >= 0 && newX < _width && !_monsters.Any(m => m != monster && m.Position == new Coordinate(newX, monster.Position.Y)))
                        {
                            newPosition = new Coordinate(newX, monster.Position.Y);
                        }
                    }
                    else
                    {
                        newPosition = new Coordinate(newX, monster.Position.Y);
                    }
                }

                monster.Move(newPosition);
            }

            // Move odd-level monsters every 2nd player step
            if (_playerMoveCounter % 2 == 0)
            {
                var oddMonsters = _monsters.Where(m => m.Level % 2 != 0).ToList();
                var shuffled = oddMonsters.OrderBy(_ => _random.Next()).ToList();

                int toMoveCount = shuffled.Count / 2;
                for (int i = 0; i < toMoveCount; i++)
                {
                    var monster = shuffled[i];
                    Coordinate oldPosition = monster.Position;
                    monster.MoveInDirection(_width, _height, _random);

                    if (monster.Position == _player.Position || _monsters.Any(m => m != monster && m.Position == monster.Position))
                    {
                        monster.Move(oldPosition);
                    }
                }
            }
        }

        public void HandlePlayerInput()
        {
            if (_temporaryMessageCounter > 0)
            {
                _temporaryMessageCounter--;
                if (_temporaryMessageCounter == 0)
                {
                    _temporaryMessage = null;
                }
            }

            Console.WriteLine("Enter move (u=up, d=down, l=left, r=right, or use arrow keys, q=quit):");
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            Console.WriteLine();

            Coordinate playerOldPosition = _player.Position;
            Coordinate playerNewPosition = playerOldPosition;

            switch (keyInfo.Key)
            {
                case ConsoleKey.U:
                case ConsoleKey.UpArrow:
                    playerNewPosition = new Coordinate(_player.Position.X, Math.Max(0, _player.Position.Y - 1));
                    break;
                case ConsoleKey.D:
                case ConsoleKey.DownArrow:
                    playerNewPosition = new Coordinate(_player.Position.X, Math.Min(_height - 1, _player.Position.Y + 1));
                    break;
                case ConsoleKey.L:
                case ConsoleKey.LeftArrow:
                    playerNewPosition = new Coordinate(Math.Max(0, _player.Position.X - 1), _player.Position.Y);
                    break;
                case ConsoleKey.R:
                case ConsoleKey.RightArrow:
                    playerNewPosition = new Coordinate(Math.Min(_width - 1, _player.Position.X + 1), _player.Position.Y);
                    break;
                case ConsoleKey.Q:
                    Console.WriteLine("Quitting game.");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid input. Use u, d, l, r, arrows, or q.");
                    return;
            }

            _player.MoveTo(playerNewPosition);
            _playerMoveCounter++;

            // Experience orb
            if (_player.Position == _experienceOrb)
            {
                Console.WriteLine("You collected an experience orb!");
                _player.GainExperience(1);
                SpawnExperienceOrb();
            }

            // Save monsters' old positions
            var oldMonsterPositions = new Dictionary<Monster, Coordinate>();
            foreach (var monster in _monsters)
            {
                oldMonsterPositions[monster] = monster.Position;
            }

            // Move monsters
            MoveMonsters();

            // Standard collision detection: Player at monster's new position
            Monster directCollision = _monsters.Find(m => m.Position == _player.Position);
            if (directCollision != null)
            {
                StartBattle(directCollision);
                return;
            }

            // Mid-step crossing collision detection
            foreach (var monster in _monsters)
            {
                Coordinate monsterOldPos = oldMonsterPositions[monster];
                Coordinate monsterNewPos = monster.Position;

                if (monsterNewPos == playerOldPosition && playerNewPosition == monsterOldPos)
                {
                    Console.WriteLine($"You collided with a level {monster.Level} monster while moving!");
                    StartBattle(monster);
                    return;
                }
            }
        }

        private void RespawnMonsters()
        {
            _monsters.Clear();
            _evenMonsterDirectionForward.Clear();

            int levelOffset = _random.Next(3, 6); // random 3 to 5
            int minMonsterLevel = _player.Level + 1;
            int maxMonsterLevel = _player.Level + levelOffset;

            for (int i = 0; i < 9; i++)
            {
                int monsterLevel = _random.Next(minMonsterLevel, maxMonsterLevel + 1);

                Coordinate position;
                do
                {
                    int x = _random.Next(0, _width);
                    int y = _random.Next(0, _height);
                    position = new Coordinate(x, y);
                } while (position == _player.Position || _monsters.Exists(m => m.Position == position));

                var monster = new Monster(position, monsterLevel);
                _monsters.Add(monster);

                if (monster.Level % 2 == 0)
                {
                    _evenMonsterDirectionForward[monster] = true;
                }
            }

            SpawnExperienceOrb();
        }

        private void StartBattle(Monster monster)
        {
            int value = monster.Level > _player.Level ? monster.Level - _player.Level : 1;

            Console.WriteLine($"\n Battle Start!");
            Console.WriteLine($"Monster Level: {monster.Level}, Player Level: {_player.Level}, Battle Value: {value}");
            Console.WriteLine("Choose your battle style:");
            Console.WriteLine("1. Guess of Fate (number guessing game)");
            Console.WriteLine("2. Arithmetic Clash (math expression)");

            int choice = 0;
            while (choice != 1 && choice != 2)
            {
                Console.Write("Enter 1 or 2: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out choice) && (choice == 1 || choice == 2))
                    break;
                Console.WriteLine("Invalid choice. Please enter 1 or 2.");
            }

            bool success = false;
            if (choice == 1)
            {
                success = GuessOfFate(value);
                if (success)
                {
                    _temporaryMessage = "!!! You won the battle and gained experience !!!";
                    _player.GainExperience(1);
                }
                else
                {
                    _temporaryMessage = "You failed the Guess of Fate! You lost 1 experience point.";
                    if (_player.Experience > 0)
                    {
                        _player.GainExperience(-1);
                    }
                }
            }
            else // choice == 2
            {
                success = ArithmeticClash(value);
                if (success)
                {
                    _temporaryMessage = "!!! You won the battle and gained experience !!!";
                    _player.GainExperience(1);
                }
                else
                {
                    ShowEndScreen();
                    return; // Exit early because game ends on failure
                }
            }

            // Remove defeated monster in any case
            _monsters.Remove(monster);

            // Respawn monsters if needed
            if (_monsters.Count == 0)
            {
                _temporaryMessage += " All monsters defeated! New wave incoming...";
                RespawnMonsters();
            }

            // Display the temporary message for 1 player turn
            _temporaryMessageCounter = 1;
        }

        private bool GuessOfFate(int value)
        {
            Console.WriteLine("\n Guess of Fate");
            Console.WriteLine($"A monster has chosen {value} numbers. One of them is the 'fate number'. Choose wisely!");

            List<int> numbers = new();
            for (int i = 0; i < value; i++)
            {
                numbers.Add(_random.Next(1, 1000)); // range 1-999 for variety
            }

            int fateIndex = _random.Next(numbers.Count);

            for (int i = 0; i < numbers.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {numbers[i]}");
            }

            int choiceIndex = -1;
            bool validInput = false;
            while (!validInput)
            {
                Console.Write("Your choice (enter number index): ");
                string input = Console.ReadLine();
                validInput = int.TryParse(input, out choiceIndex) && choiceIndex >= 1 && choiceIndex <= numbers.Count;
                if (!validInput)
                    Console.WriteLine("Please enter a valid index number.");
            }

            return choiceIndex - 1 == fateIndex;
        }

        private bool ArithmeticClash(int value)
        {
            Console.WriteLine("\n Arithmetic Clash");

            int result = _random.Next(1, 50);
            string expression = result.ToString();

            if (value == 1)
            {
                Console.WriteLine($"{result} = ?");
            }
            else
            {
                Console.WriteLine($"Solve this expression with {value} steps!");
                for (int i = 0; i < value; i++)
                {
                    char[] ops = { '+', '-', '*', '/' };
                    char op = ops[_random.Next(ops.Length)];
                    int operand = _random.Next(1, 10);

                    expression += $" {op} {operand}";

                    result = op switch
                    {
                        '+' => result + operand,
                        '-' => result - operand,
                        '*' => result * operand,
                        '/' => operand != 0 ? result / operand : result,
                        _ => result
                    };
                }

                Console.WriteLine($"{expression} = ?");
            }

            int playerAnswer = 0;
            bool validInput = false;
            while (!validInput)
            {
                Console.Write("Your answer: ");
                string input = Console.ReadLine();
                validInput = int.TryParse(input, out playerAnswer);
                if (!validInput)
                    Console.WriteLine("Please enter a valid number.");
            }

            return playerAnswer == result;
        }

        private void ShowEndScreen()
        {
            Console.Clear();
            Console.WriteLine("\n You have fallen in battle...");
            Console.WriteLine($"Your final level (score): {_player.Level}");
            Console.WriteLine("1) Play again");
            Console.WriteLine("2) Quit");

            int choice = 0;
            while (choice != 1 && choice != 2)
            {
                Console.Write("Enter 1 or 2: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out choice) && (choice == 1 || choice == 2))
                    break;
                Console.WriteLine("Invalid input. Enter 1 or 2.");
            }

            if (choice == 1)
            {
                _player = new Player(new Coordinate(0, 0));
                InitializeGameObjects();
                _temporaryMessage = "A new adventure begins!";
                _temporaryMessageCounter = 1;
            }
            else
            {
                Console.WriteLine("Thank you for playing!");
                Environment.Exit(0);
            }
        }

        public void GameLoop()
        {
            while (true)
            {
                Console.Clear();
                RenderObjects();
                PrintMap();
                HandlePlayerInput();
            }
        }
    }
}
