using System;

namespace ChangeBrightness
{
    class Program
    {
        private BrightnessFileReader reader = new BrightnessFileReader();

        static void Main(string[] args)
        {
            var app = new Program();
            app.printWelcome();
            app.printBrightnessStatus();
            app.RunMainMenu();
        }

        private void printWelcome()
        {
            Console.WriteLine("Welcome!");
            Console.WriteLine("Let's change the brightness of your screen.\n");
        }

        private void printBrightnessStatus()
        {
            Console.WriteLine("Current brightness: " + reader.ActualBrightness);
            Console.WriteLine("Maximum brightness: " + reader.MaxBrightness);
        }

        private void RunMainMenu()
        {
            Console.WriteLine("\nPress [Y] to change the current brightness.");
            Console.WriteLine("Press any other key to exit.");
            Console.Write("\nWhat would you like to do?: ");

            var option = Console.ReadKey().Key;
            Console.WriteLine(); // Adds a missing empty line for next menus.

            if (option == ConsoleKey.Y)
            {
                RunChangeBrightnessMenu();
            }
        }

        private void RunChangeBrightnessMenu()
        {
            var writer = new BrightnessFileWriter(reader);
            var brightness = ReadBrightness("Enter the new brightness value: ");

            try
            {
                writer.ChangeTo(brightness);
            }
            catch (BrightnessOutOfRangeException)
            {
                Console.WriteLine("The given brightness is not valid.");
                Console.WriteLine("Please enter a value between 0 and " + reader.MaxBrightness);
            }
        }

        private static int ReadBrightness(string message)
        {
            if (message != null)
            {
                Console.Write(message);
            }

            int value;
            var success = int.TryParse(Console.ReadLine(), out value);
            return success ? value : ReadBrightness("Please enter a valid integer: ");
        }
    }

    interface BrightnessValidator
    {
        void CheckBrightnessValue(int value);
    }

    class BrightnessFileReader : BrightnessValidator
    {
        private const string ActualBrightnessPath = @"/sys/class/backlight/intel_backlight/actual_brightness";

        private const string MaxBrightnessPath = @"/sys/class/backlight/intel_backlight/max_brightness";

        public int ActualBrightness
        {
            get
            {
                var brightness = System.IO.File.ReadAllText(ActualBrightnessPath);
                return int.Parse(brightness);
            }
        }

        public int MaxBrightness
        {
            get
            {
                var maxBrightness = System.IO.File.ReadAllText(MaxBrightnessPath);
                return int.Parse(maxBrightness);
            }
        }

        public void CheckBrightnessValue(int value)
        {
            if (!ValidBrightness(value))
            {
                throw new BrightnessOutOfRangeException();
            }
        }

        private bool ValidBrightness(int value)
        {
            return value >= 0 && value <= MaxBrightness;
        }
    }

    public class BrightnessOutOfRangeException : Exception {}

    class BrightnessFileWriter
    {
        private const string BrightnessPath = @"/sys/class/backlight/intel_backlight/brightness";

        private BrightnessValidator validator;

        public BrightnessFileWriter(BrightnessValidator validator)
        {
            this.validator = validator;
        }

        public void ChangeTo(int newValue)
        {
            validator.CheckBrightnessValue(newValue);
            WriteBrightness(newValue);
        }

        private void WriteBrightness(int value)
        {
            System.IO.File.WriteAllText(BrightnessPath, value.ToString());
        }
    }
}
