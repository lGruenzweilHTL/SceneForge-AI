public class AIResponse
{
    public string model { get; set; }
    public string created_at { get; set; }
    public AIMessage message { get; set; }
    public string done_reason { get; set; }
    public bool done { get; set; }
    public ulong total_duration { get; set; }
    public ulong load_duration { get; set; }
    public ulong prompt_eval_count { get; set; }
    public ulong prompt_eval_duration { get; set; }
    public ulong eval_count { get; set; }
    public ulong eval_duration { get; set; }
}