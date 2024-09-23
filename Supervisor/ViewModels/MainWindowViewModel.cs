﻿using System;
using System.Collections.Generic;
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

namespace Supervisor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public float? cpuUsage;
        [ObservableProperty]
        public string? ramUsage;
        [ObservableProperty]
        public float ramUsagePercentage;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private DispatcherTimer timer;
        private float totalRamInMB;

        public MainWindowViewModel()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            totalRamInMB = GetTotalRamInMB();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100),
            };
            timer.Tick += GetCpuUsage;
            timer.Tick += GetRamUsage;
            timer.Start();
        }

        private void GetCpuUsage(object sender, EventArgs e)
        {
            CpuUsage = cpuCounter.NextValue();
        }

        private void GetRamUsage(object sender, EventArgs e)
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
    }
}
