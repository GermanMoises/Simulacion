using Simulacion.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Simulacion.ViewModels;

public class SimulationViewModel : INotifyPropertyChanged
{
    private readonly Random _random = new Random();
    private readonly List<DemandDistribution> _demandProbabilities;

    // Valores fijos e invisibles
    private const int MaxInventory = 25;
    private const int ReviewPeriod = 7;

    // Propiedades editables
    public int LeadTime { get; set; } = 2;
    public decimal LostSaleCost { get; set; } = 15m; // REINCORPORADO
    public int SimulationDays { get; set; } = 30;
    public int SimulationRuns { get; set; } = 100;

    // Promedios Globales
    private decimal _averageCost; // REINCORPORADO
    public decimal AverageCost { get => _averageCost; set { _averageCost = value; OnPropertyChanged(); } }

    private double _averageLostSales;
    public double AverageLostSales { get => _averageLostSales; set { _averageLostSales = value; OnPropertyChanged(); } }

    private double _averageFinalInventory;
    public double AverageFinalInventory { get => _averageFinalInventory; set { _averageFinalInventory = value; OnPropertyChanged(); } }

    public ObservableCollection<SimulationDay> SimulationResults { get; set; } = new();

    public ICommand RunSimulationCommand { get; }

    public SimulationViewModel()
    {
        _demandProbabilities = new List<DemandDistribution>
        {
            new() { Demand = 0, LowerLimit = 0.00, UpperLimit = 0.01 },
            new() { Demand = 1, LowerLimit = 0.01, UpperLimit = 0.02 },
            new() { Demand = 2, LowerLimit = 0.02, UpperLimit = 0.05 },
            new() { Demand = 3, LowerLimit = 0.05, UpperLimit = 0.15 },
            new() { Demand = 4, LowerLimit = 0.15, UpperLimit = 0.55 },
            new() { Demand = 5, LowerLimit = 0.55, UpperLimit = 1.00 }
        };

        RunSimulationCommand = new Command(ExecuteSimulation);
    }

    private void ExecuteSimulation()
    {
        decimal globalTotalCost = 0;
        int globalTotalLostSales = 0;
        int globalTotalFinalInventory = 0;

        List<SimulationDay> lastRunDetails = new();

        for (int r = 1; r <= SimulationRuns; r++)
        {
            int currentInventory = MaxInventory;
            int pendingOrder = 0;
            int arrivalDay = -1;

            decimal runTotalCost = 0;
            int runLostSales = 0;
            int runFinalInventorySum = 0;

            lastRunDetails.Clear();

            for (int i = 0; i <= SimulationDays; i++)
            {
                var dayRecord = new SimulationDay { Day = i };

                if (i == arrivalDay)
                {
                    dayRecord.ReceivedGoods = pendingOrder;
                    currentInventory += pendingOrder;
                    pendingOrder = 0;
                }
                else
                {
                    dayRecord.ReceivedGoods = 0;
                }

                dayRecord.InitialInventory = currentInventory;

                if (i == 0)
                {
                    dayRecord.Demand = 0;
                    dayRecord.RandomValue = 0;
                    dayRecord.FinalInventory = currentInventory;
                    dayRecord.LostSalesCost = 0;
                }
                else
                {
                    dayRecord.RandomValue = _random.NextDouble();
                    dayRecord.Demand = GetDemandFromRandom(dayRecord.RandomValue);

                    if (currentInventory >= dayRecord.Demand)
                    {
                        dayRecord.FinalInventory = currentInventory - dayRecord.Demand;
                        dayRecord.LostSales = 0;
                    }
                    else
                    {
                        dayRecord.FinalInventory = 0;
                        dayRecord.LostSales = dayRecord.Demand - currentInventory;
                    }

                    dayRecord.LostSalesCost = dayRecord.LostSales * LostSaleCost;

                    runLostSales += dayRecord.LostSales;
                    runTotalCost += dayRecord.LostSalesCost;
                }

                if (i > 0 && i % ReviewPeriod == 0)
                {
                    dayRecord.IsReviewDay = true;
                    dayRecord.OrderedQuantity = MaxInventory - dayRecord.FinalInventory;
                    pendingOrder = dayRecord.OrderedQuantity;
                    arrivalDay = i + LeadTime;
                    dayRecord.ArrivalDay = arrivalDay;
                }

                currentInventory = dayRecord.FinalInventory;
                runFinalInventorySum += dayRecord.FinalInventory;

                lastRunDetails.Add(dayRecord);
            }

            globalTotalLostSales += runLostSales;
            globalTotalFinalInventory += (runFinalInventorySum / Math.Max(1, SimulationDays));
            globalTotalCost += runTotalCost;
        }

        AverageLostSales = (double)globalTotalLostSales / SimulationRuns;
        AverageFinalInventory = (double)globalTotalFinalInventory / SimulationRuns;
        AverageCost = globalTotalCost / SimulationRuns;

        Application.Current.Dispatcher.Dispatch(() =>
        {
            SimulationResults.Clear();
            foreach (var item in lastRunDetails)
            {
                SimulationResults.Add(item);
            }
        });
    }

    private int GetDemandFromRandom(double randomValue)
    {
        var dist = _demandProbabilities.FirstOrDefault(d => randomValue >= d.LowerLimit && randomValue < d.UpperLimit);
        return dist != null ? dist.Demand : 5;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}