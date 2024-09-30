using System;
using System.Collections.Generic;
using System.Management;

class Program
{
    // Dictionary to store currently connected devices
    static Dictionary<string, string> connectedDevices = new Dictionary<string, string>();

    static void Main(string[] args)
    {
        // Initialize the list with currently connected devices
        InitializeConnectedDevices();

        // Create an event watcher for device change events (e.g., devices inserted/removed)
        ManagementEventWatcher watcher = new ManagementEventWatcher();
        watcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");

        // Set the event handler to detect when a new device is connected or disconnected
        watcher.EventArrived += new EventArrivedEventHandler(DeviceChanged);

        // Start watching for device change events
        watcher.Start();

        Console.WriteLine("Monitoring new device connections. Press Enter to exit.");
        Console.ReadLine();

        // Stop the watcher when exiting
        watcher.Stop();
    }

    // This method is called when a device is inserted or removed
    static void DeviceChanged(object sender, EventArrivedEventArgs e)
    {
        // EventType 2 means device inserted, 3 means device removed
        string eventType = e.NewEvent.Properties["EventType"].Value.ToString();
        if (eventType == "2")
        {
            Console.WriteLine("A new device has been connected.");
            CheckForNewDevices();
        }
        else if (eventType == "3")
        {
            Console.WriteLine("A device has been disconnected.");
        }
    }

    // Initialize the list of connected devices at startup
    static void InitializeConnectedDevices()
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
        foreach (ManagementObject device in searcher.Get())
        {
            string deviceID = device["DeviceID"]?.ToString();
            string deviceName = device["Name"]?.ToString();

            if (!string.IsNullOrEmpty(deviceID) && !connectedDevices.ContainsKey(deviceID))
            {
                connectedDevices.Add(deviceID, deviceName);
            }
        }
    }

    // Method to check for new devices
    static void CheckForNewDevices()
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
        foreach (ManagementObject device in searcher.Get())
        {
            string deviceID = device["DeviceID"]?.ToString();
            string deviceName = device["Name"]?.ToString();

            // If the device is not already in the list, it is a newly connected device
            if (!string.IsNullOrEmpty(deviceID) && !connectedDevices.ContainsKey(deviceID))
            {
                // Add the new device to the list
                connectedDevices.Add(deviceID, deviceName);

                // Display details of the new device
                Console.WriteLine($"New Device Connected: {deviceName}");
                Console.WriteLine($"Device ID: {deviceID}");
                Console.WriteLine("-----------------------------");
            }
        }
    }
}
