using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MainAppSettings
{
    [Category("Simulation settings")]
    [DisplayName("Number of Monte Carlo iterations")]
    public int NumberOfMonteCarloIterations { get; set; } = 10000;

    [Category("Simulation settings")]
    [DisplayName("Standard Deviation")]
    public double StandardDeviation { get; set; } = 0.05;
    [Category("General settings")]
    [DisplayName("Technical gates")]
    public bool Technicalgates { get; set; } = false;
    [Category("General settings")]
    [DisplayName("Auto sort tree after paste")]
    public bool AutoSortTree { get; set; } = true;

    public static MainAppSettings Current { get; set; } = new MainAppSettings();
}