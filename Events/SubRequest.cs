﻿namespace Events
{
    public class SubRequest
    {
        public Dictionary<string, object> Headers { get; set; } = new();
        public float val1 { get; set; }
        public float val2 { get; set; }
        public string operation => "Sub";
        public Guid guid => Guid.NewGuid();
    }
}
