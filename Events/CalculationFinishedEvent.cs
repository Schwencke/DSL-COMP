namespace Events
{
    public class CalculationFinishedEvent
    {
        public Dictionary<string, object> Headers { get; set; } = new();
        public float result { get; set; }
        public float val1 { get; set; }
        public float val2 { get; set; }
        public string operation => "Add";
        public string trigger { get; set; }
        public Guid guid => Guid.NewGuid();
    }
}
