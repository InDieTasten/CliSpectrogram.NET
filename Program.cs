using System;
using NAudio.Wave;

namespace CliSpectogram.NET
{
    class Program
    {
        static int? selectedInputDeviceIndex = null;
        static int Main(string[] args)
        {
            Console.WriteLine("Welcome to the CLI Spectogram.NET Tool");

            int returnCode = SelectInputDeviceId();
            if (returnCode != 0)
            {
                return returnCode;
            }

            Console.Write($"Selected Device: {selectedInputDeviceIndex.Value}");

            return 0;
        }

        private static int SelectInputDeviceId()
        {
            if (WaveIn.DeviceCount == 0)
            {
                Console.Error.WriteLine("You do not have any input devices to visualize.");
                return -1;
            }

            if (WaveIn.DeviceCount == 1)
            {
                Console.WriteLine("Selected the only input device available for visualization:");
                Console.WriteLine(WaveIn.GetCapabilities(0).ProductName);
                selectedInputDeviceIndex = 0;
            }
            else
            {
                Console.WriteLine("Please select your input device:");

                do
                {
                    for (int inputDeviceIndex = 0; inputDeviceIndex < WaveIn.DeviceCount; inputDeviceIndex++)
                    {
                        Console.WriteLine($"{inputDeviceIndex}: {WaveIn.GetCapabilities(inputDeviceIndex).ProductName}");
                    }
                    var suggestedInputDevice = Console.ReadLine();
                    if (int.TryParse(suggestedInputDevice, out int parsedSuggestedDeviceIndex))
                    {
                        if (parsedSuggestedDeviceIndex >= 0 && parsedSuggestedDeviceIndex < WaveIn.DeviceCount)
                        {
                            selectedInputDeviceIndex = parsedSuggestedDeviceIndex;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please provide a valid device index from the list.");
                    }
                }
                while (!selectedInputDeviceIndex.HasValue);
            }

            return 0;
        }
    }
}
