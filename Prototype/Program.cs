using System;
using System.IO.Ports;
using System.Threading;

namespace RFIDReaderApp
{
    class Program
    {
        static SerialPort serialPort = new SerialPort("COM3", 57600, Parity.None, 8, StopBits.One);
        static ManualResetEvent dataReceivedEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            serialPort.ReadBufferSize = 16384;
            Console.WriteLine($"Current Read Buffer Size: {serialPort.ReadBufferSize} bytes");
            serialPort.DataReceived += SerialPort_DataReceived;

            Console.WriteLine("Press [Enter] to activate the RFID reader. Press [Ctrl+C] to exit.");
            //bool isReaderActive = false;

            while (true)
            {
                // Wait for the Enter key to be pressed
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // Check if the key pressed was Enter
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (!serialPort.IsOpen) // Check if the reader is not already active and the port is closed
                    {
                        try
                        {
                            // Reset the ManualResetEvent so that it blocks the main thread
                            dataReceivedEvent.Reset();
                            serialPort.Open();
                            //isReaderActive = true; // Set the reader as active 
                            Console.WriteLine("Port opened. Waiting for data...");

                            // Wait until data is received and processed
                            dataReceivedEvent.WaitOne();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        finally
                        {
                            if (serialPort.IsOpen)
                            {
                                serialPort.Close();
                                //isReaderActive = false; // Set the reader as inactive
                                Console.WriteLine("Port closed. Press [Enter] to activate the RFID reader again.");
                            }
                        }
                    
                        //Console.WriteLine("Reader is already active. Press [Enter] again to deactivate and activate again.");
                    }
                    else if (serialPort.IsOpen)
                    {
                        Console.WriteLine("Port is already open. Press [Enter] again to activate the RFID reader.");
                    }
                }
            }
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            byte[] buffer = new byte[bytesToRead];
            sp.Read(buffer, 0, bytesToRead);

            // Extract the consistent portion of the UII, excluding the last two bytes
            string fullData = BitConverter.ToString(buffer);
            if (fullData.Length > 6)
            {
                string consistentUII = fullData.Substring(0, fullData.Length - 0
                );
                Console.WriteLine(consistentUII);
            }
            else
            {  
                Console.WriteLine(fullData);
            }

            // Signal that data has been received and processed
            dataReceivedEvent.Set();
        }
    }
}

