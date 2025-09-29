using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Labyritghrtn
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Width:");
            int.TryParse(Console.ReadLine(), out int width);
            Console.WriteLine("Height:");
            int.TryParse(Console.ReadLine(), out int height);

            if (width <= 0 || height <= 0)
                throw new Exception("Invalid width or height!");

            char[,] mazeArray = new char[height, width];
            for (int i = 0; i < height; i++)
            {
                string? mazeLine = Console.ReadLine();
                if (mazeLine == null)
                    throw new Exception("Invalid maze input!");

                for (int j = 0; j < width; j++)
                    mazeArray[i, j] = mazeLine[j];
            }

            Maze maze = new Maze(width, height, mazeArray);

            Console.WriteLine();
            Console.WriteLine("Starting conditions:");
            Console.WriteLine(maze);

            Console.WriteLine();
            Console.WriteLine("Steps:");
            for (int i = 0; i < 20; i++)
            {
                maze.StepAllMonsters();
                Console.WriteLine($"{i + 1}. krok");
                Console.WriteLine(maze);
            }
        }
    }

    class Maze
    {
        public int Width;
        public int Height;
        public char[,] MazeArray;
        public List<Monster> MonsterList;

        public Maze(int width, int height, char[,] mazeArray)
        {
            Width = width;
            Height = height;
            MazeArray = mazeArray;

            List<Vector2> monsterPositions = FindAllMonsters();
            List<Monster> monsterList = new List<Monster>();
            foreach (Vector2 position in monsterPositions)
            {
                Monster newMonster = new Monster(this, position);
                monsterList.Add(newMonster);
            }

            MonsterList = monsterList;
        }

        public bool IsPositionEmpty(Vector2 position)
        {
            return MazeArray[(int)position.Y, (int)position.X] == '.';
        }

        public bool IsPositionValid(Vector2 position)
        {
            if (position.Y < 0 || position.Y >= Height)
                return false;
            if (position.X < 0 || position.X >= Width)
                return false;

            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                    sb.Append(MazeArray[i, j]);

                if (i < Width - 1)
                    sb.Append('\n');
            }

            return sb.ToString();
        }

        public List<Vector2> FindAllMonsters()
        {
            List<Vector2> monsterPositions = new List<Vector2>();

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    char ch = MazeArray[i, j];
                    if (Monster.CharToOrientation.ContainsKey(ch))
                    {
                        monsterPositions.Add(new Vector2(j, i));
                    }
                }
            }

            return monsterPositions;
        }

        public void StepAllMonsters()
        {
            foreach (Monster monster in MonsterList)
            {
                monster.Step();
            }
        }
    }

    class Monster
    {
        public static Dictionary<Vector2, char> OrientationToChar = new Dictionary<Vector2, char>
        {
            { new Vector2(-1, 0), '<' },
            { new Vector2(1, 0), '>' },
            { new Vector2(0, -1), '^' },
            { new Vector2(0, 1), 'v' },
        };
        public static Dictionary<Vector2, string> OrientationToName = new Dictionary<Vector2, string>
        {
            { new Vector2(-1, 0), "LEFT" },
            { new Vector2(1, 0), "RIGHT" },
            { new Vector2(0, -1), "UP" },
            { new Vector2(0, 1), "DOWN" },
        };
        public static Dictionary<char, Vector2> CharToOrientation = new Dictionary<char, Vector2>
        {
            { '<', new Vector2(-1, 0) },
            { '>', new Vector2(1, 0) },
            { '^', new Vector2(0, -1) },
            { 'v', new Vector2(0, 1) },
        };

        private Boolean _rotatedLastStep;

        // Backing fields
        private Vector2 _position;
        private Vector2 _orientation;
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != Vector2.Zero)
                {
                    Maze.MazeArray[(int)_position.Y, (int)_position.X] = '.';
                    Maze.MazeArray[(int)value.Y, (int)value.X] = OrientationToChar[Orientation];
                }

                _position = value;
            }
        }
        public Vector2 Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation != Vector2.Zero)
                {
                    Maze.MazeArray[(int)_position.Y, (int)_position.X] = OrientationToChar[value];
                }

                _orientation = value;
            }
        }
        public Maze Maze;

        public Monster(Maze maze, Vector2 position)
        {
            char monsterChar = maze.MazeArray[(int)((Vector2)position).Y, (int)((Vector2)position).X];
            Maze = maze;
            Position = position;
            Orientation = Monster.CharToOrientation[monsterChar];
        }

        public void Step()
        {
            Vector2 nextPosition = Position + Orientation;

            // Rotate right
            Vector2 rightOrientation = new Vector2(-Orientation.Y, Orientation.X);
            Vector2 rightPosition = Position + rightOrientation;

            // Move forward if valid
            if (
                (Maze.IsPositionValid(nextPosition) && Maze.IsPositionEmpty(nextPosition))
                && (_rotatedLastStep || (Maze.IsPositionValid(rightPosition) && !Maze.IsPositionEmpty(rightPosition)))
                )
            {
                _rotatedLastStep = false;
                Position = nextPosition;
                return;
            }

            _rotatedLastStep = true;

            // Rotate right if it will lead to a valid step forward next Step()
            if (Maze.IsPositionValid(rightPosition) && Maze.IsPositionEmpty(rightPosition))
            {
                Orientation = rightOrientation;
                return;
            }

            // Rotate left otherwise
            Orientation = new Vector2(Orientation.Y, -Orientation.X);
        }

        public override string ToString()
        {
            return $"Monster Object at {Position} looking {Monster.OrientationToName[Orientation]}";
        }
    }
}