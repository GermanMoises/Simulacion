using System;
using System.Collections.Generic;
using System.Text;

namespace Simulacion.Models
{
    public class SimulationDay
    {
        public int Day { get; set; }
        public int ReceivedGoods { get; set; }
        public int InitialInventory { get; set; }
        public double RandomValue { get; set; }
        public int Demand { get; set; }
        public int FinalInventory { get; set; }
        public int LostSales { get; set; }
        public decimal LostSalesCost { get; set; }
        public bool IsReviewDay { get; set; }
        public int OrderedQuantity { get; set; }
        public int? ArrivalDay { get; set; }
    }
}
