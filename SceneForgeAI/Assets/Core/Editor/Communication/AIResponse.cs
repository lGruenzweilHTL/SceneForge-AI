using Unity.Plastic.Newtonsoft.Json;

public class AIResponse
{
    public string model { get; set; }
    public string created_at { get; set; }
    public AIMessage message { get; set; }
    public string done_reason { get; set; }
    public bool done { get; set; }
    public int total_duration { get; set; }
    public int load_duration { get; set; }
    public int prompt_eval_count { get; set; }
    public int prompt_eval_duration { get; set; }
    public int eval_count { get; set; }
    public int eval_duration { get; set; }
}