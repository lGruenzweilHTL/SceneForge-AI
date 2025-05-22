public class AIResponse
{
    public string model;
    public string created_at;
    public string response;
    public bool done;
    public string done_reason;
    public int[] context;
    public int total_duration;
    public int load_duration;
    public int prompt_eval_count;
    public int prompt_eval_duration;
    public int eval_count;
    public int eval_duration;
}