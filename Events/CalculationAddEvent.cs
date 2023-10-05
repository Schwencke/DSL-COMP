namespace Events
{
    public class CalculationAddEvent
    {
        public Dictionary<string, object> Headers { get; set; } = new();
        public float val1 { get; set; }
        public float val2 { get; set; }
        public string operation => "Add";
        public Guid guid { get; set; }
    }
}