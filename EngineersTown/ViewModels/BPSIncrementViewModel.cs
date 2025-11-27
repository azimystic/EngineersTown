namespace EngineersTown.Models.ViewModels
{
    public class BPSIncrementViewModel
    {
        public List<BPSItemViewModel> BPSItems { get; set; } = new List<BPSItemViewModel>();
    }

    public class BPSItemViewModel
    {
        public int BPS { get; set; }
        public decimal IncrementAmount { get; set; }
        public int EmployeeCount { get; set; }
    }
}