namespace EngineersTown.Models.ViewModels
{
    public class AdhocIncrementViewModel
    {
        public decimal Percentage { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalBasicSalary { get; set; }
        public decimal CurrentTotalAdhoc { get; set; }
    }

    public class AdhocPreviewItem
    {
        public string EmployeeName { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public decimal CurrentAdhoc { get; set; }
        public decimal AdhocIncrement { get; set; }
        public decimal NewAdhoc { get; set; }
    }
}