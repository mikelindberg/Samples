using System;
using System.IO.Ports;
using Newtonsoft.Json;

namespace controller_interface_module
{
    public class SerialInterface
    {
        static SerialPort _serialPort;

        public SerialInterface()
        {
            _serialPort = new SerialPort();
        }

        public bool OpenSerialConnection()
        {
            try
            {
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return _serialPort.IsOpen;
        }

        public bool CloseSerialConnection()
        {
            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return _serialPort.IsOpen;
        }

        // Display Port values and prompt user to enter a port.
        public string SetPortName(string portName)
        {
            try
            {
                _serialPort.PortName = portName;
                Console.WriteLine("Port " + portName + " selected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed setting port name");
            }

            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public int SetPortBaudRate(int baudRate)
        {
            try
            {
                _serialPort.BaudRate = baudRate;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed setting baudrate");
            }

            return baudRate;
        }

        public string getAvailablePortsProperty()
        {
            //Get available ports reportedProperty
            string availablePorts = "Available ports: ";
            foreach (string portName in SerialPort.GetPortNames())
            {
                availablePorts += portName + ", ";
            }

            return availablePorts;
        }

        public string ReadSerial()
        {
            if (_serialPort.IsOpen)
                return _serialPort.ReadLine();
            else return "";
        }

        public void WriteSerial(Command command)
        {
            var commandString = JsonConvert.SerializeObject(command);
            _serialPort.WriteLine(commandString);
        }
    }
}