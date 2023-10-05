namespace Events
{
    public class Result
    {
        public Dictionary<string, object> Headers { get; set; } = new(); public float result { get; set; }
        public float val1 { get; set; }
        public float val2 { get; set; }
        public string operation { get; set; }
        public Guid id { get; set; }
    }
}
