namespace Order.Model
{
    public class MonthlyProfit
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal Profit => TotalPrice - TotalCost;
    }
}
