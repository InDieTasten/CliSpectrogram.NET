using System;
using System.Linq;
using NAudio.Wave;

namespace CliSpectogram.NET
{
    class Program
    {
        static int? selectedInputDeviceIndex = null;
        static WaveInEvent inputDevice;
        static int Main(string[] args)
        {
            Console.WriteLine("Welcome to the CLI Spectogram.NET Tool");

            int returnCode = SelectInputDeviceId();
            if (returnCode != 0)
            {
                return returnCode;
            }
            Console.WriteLine($"Selected Device: {selectedInputDeviceIndex.Value}");

            using (inputDevice = new WaveInEvent())
            {
                inputDevice.DeviceNumber = selectedInputDeviceIndex.Value;
                inputDevice.BufferMilliseconds = 50;
                PrintInputDeviceSettings();

                Console.WriteLine("To start the visualization, press a key...");
                Console.ReadKey(true);

                inputDevice.DataAvailable += InputDevice_DataAvailable;
                inputDevice.StartRecording();

                Console.WriteLine("Listening on your input device. Press a key to stop...");
                Console.ReadKey(true);
                
                inputDevice.StopRecording();
                inputDevice.DataAvailable -= InputDevice_DataAvailable;

                Console.WriteLine("Done recording.");
            }

            return 0;
        }

        private static void PrintInputDeviceSettings()
        {
            Console.WriteLine("Listener settings:");
            Console.WriteLine($" WaveFormat: {inputDevice.WaveFormat}");
            Console.WriteLine($" BufferCount: {inputDevice.NumberOfBuffers}");
            Console.WriteLine($" BufferMilliseconds: {inputDevice.BufferMilliseconds}");
            Console.WriteLine("");
        }

        private static void InputDevice_DataAvailable(object sender, WaveInEventArgs e)
        {
            // peak renderer
            float max = 0;
            var buffer = new WaveBuffer(e.Buffer);
            // interpret as 16 bit signed point audio
            for (int index = 0; index < e.BytesRecorded / 2; index++)
            {
                var sample = buffer.ShortBuffer[index];

                float sample32 = sample / (float)short.MaxValue;
                // absolute value 
                if (sample32 < 0)
                    sample32 = -sample32;
                // is this the max value?
                if (sample32 > max)
                    max = sample32;
            }

            int maxWidth = Console.BufferWidth;
            int mappedValue = (int)(maxWidth * max);
            string bar = "";
            for (int i = 0; i < mappedValue; i++)
                bar += "#";

            Console.WriteLine(bar);
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
