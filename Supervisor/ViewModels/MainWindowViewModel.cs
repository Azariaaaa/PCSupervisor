﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using Supervisor.Services;

namespace Supervisor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private float? cpuUsage;
        [ObservableProperty]
        private string? ramUsage;
        [ObservableProperty]
        private float ramUsagePercentage;
        [ObservableProperty]
        private string systemUpTime;
        [ObservableProperty]
        private string processTotal;
        [ObservableProperty]
        private string threadTotal;
        [ObservableProperty]
        private string cpuName;
        [ObservableProperty]
        private string pcName;
        [ObservableProperty]
        private string gpuName;
        [ObservableProperty]
        private ObservableCollection<string> cpusTemperatures;

        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private PerformanceCounter systemUpTimeCounter;
        private PerformanceCounter processCounter;
        private PerformanceCounter threadCounter;
        Computer Computer { get; set; }
        UpdateVisitor UpdateVisitor { get; set; }
        private DispatcherTimer timer;
        private float totalRamInMB;

        public MainWindowViewModel()
        {
            Computer = new Computer();
            UpdateVisitor = new UpdateVisitor();
            CpusTemperatures = new ObservableCollection<string>();

            Computer.IsCpuEnabled = true;
            Computer.Open();
            Computer.Accept(UpdateVisitor);

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            systemUpTimeCounter = new PerformanceCounter("System", "System Up Time");
            processCounter = new PerformanceCounter("System", "Processes");
            threadCounter = new PerformanceCounter("System", "Threads");

            totalRamInMB = GetTotalRamInMB();
            GetInformations();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(355),
            };
            timer.Tick += GetCpuTemperatures;
            timer.Tick += GetCpuUsage;
            timer.Tick += GetRamUsage;
            timer.Tick += GetSystemUpTime;
            timer.Tick += GetTotalProcess;
            timer.Tick += GetTotalThreads;
            timer.Start();
        }
        private void GetCpuTemperatures(object? sender, EventArgs e)
        {
            CpusTemperatures.Clear();

            foreach (IHardware hardware in Computer.Hardware)
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        CpusTemperatures.Add($"{sensor.Name}: {sensor.Value.GetValueOrDefault()} °C");
                    }
                }
            }
        }

        private void GetCpuUsage(object? sender, EventArgs e)
        {
            CpuUsage = cpuCounter.NextValue();
        }

        private void GetRamUsage(object? sender, EventArgs e)
        {
            float availableRamInMB = ramCounter.NextValue();
            float usedRamInMB = totalRamInMB - availableRamInMB;
            RamUsage = $"{usedRamInMB:F0} MB used / {totalRamInMB:F0} MB total";

            RamUsagePercentage = (usedRamInMB / totalRamInMB) * 100;
        }

        private float GetTotalRamInMB()
        {
            float totalRam = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in searcher.Get())
            {
                totalRam += Convert.ToSingle(obj["Capacity"]) / (1024 * 1024);
            }
            return totalRam;
        }

        private void GetSystemUpTime(object? sender, EventArgs e)
        {
            float systemUpTimeInSeconds = systemUpTimeCounter.NextValue();
            TimeSpan upTime = TimeSpan.FromSeconds(systemUpTimeInSeconds);
            SystemUpTime = $"{upTime.Hours:D2}:{upTime.Minutes:D2}:{upTime.Seconds:D2}";
        }

        private void GetTotalProcess(object? sender, EventArgs e)
        {
            ProcessTotal = Convert.ToString(processCounter.NextValue());
        }

        private void GetTotalThreads(object? sender, EventArgs e)
        {
            ThreadTotal = Convert.ToString(threadCounter.NextValue());
        }

        private void GetInformations()
        {
            GetCpuName();
            GetPcName();
            GetGpuName();
        }

        private void GetCpuName()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");

            foreach (ManagementObject obj in searcher.Get())
            {
                CpuName = obj["Name"].ToString();
            }
        }

        private void GetPcName()
        {
            PcName = Environment.MachineName;
        }

        private void GetGpuName()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");

            foreach (ManagementObject obj in searcher.Get())
            {
                GpuName = obj["Name"].ToString();
            }
        }
    }
}
