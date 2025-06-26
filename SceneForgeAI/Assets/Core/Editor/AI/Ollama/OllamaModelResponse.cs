using System.Collections.Generic;

public class OllamaModelResponse
{
    public List<ModelData> Models { get; set; }
    
    public class ModelData
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string modified_at { get; set; }
        public ulong Size { get; set; }
        public string Digest { get; set; }
        public ModelDetails Details { get; set; }
    }
    
    public class ModelDetails
    {
        public string parent_model { get; set; }
        public string Format { get; set; }
        public string Family { get; set; }
        public List<string> Families { get; set; }
        public string parameter_size { get; set; }
        public string quantization_level { get; set; }
    }
}