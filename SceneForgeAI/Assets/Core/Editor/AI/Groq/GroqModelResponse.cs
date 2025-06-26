using System.Collections.Generic;

public class GroqModelResponse
{
    public string Object { get; set; }
    public List<ModelData> Data { get; set; }

    public class ModelData
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public ulong Created { get; set; }
        public string owned_by { get; set; }
        public bool Active { get; set; }
        public int context_window { get; set; }
        public object public_apps { get; set; }
        public ulong max_completion_tokens { get; set; }
    }
}