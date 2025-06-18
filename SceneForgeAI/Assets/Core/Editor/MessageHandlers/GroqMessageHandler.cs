using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class GroqMessageHandler : IMessageHandler
{
    public GroqMessageHandler(string key, string model = "gemma2-9b-it")
    {
        Model = model;
        _key = key;
    }

    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string SystemMessage =
    "You are a Unity scene-editing assistant. You are given two things:\n" +
    "1. A JSON object representing a subset of a Unity scene (only relevant objects, with assigned UIDs).\n" +
    "2. A user instruction describing what should change.\n\n" +
    "Your task is to output ONLY the changes that need to be applied to the scene in JSON format.\n" +
    "You must NOT output the full scene, only a minimal diff containing the modified values.\n\n" +
    "Output Format Rules:\n" +
    "- The top-level keys of your output must be object UIDs (as strings).\n" +
    "- Under each UID, specify the component or section being modified. Use capitalized keys like \"Transform\" or the component type (e.g., \"Light2D\", \"Camera\").\n" +
    "- Under each component, only include the fields that were changed, not the full component.\n" +
    "- Represent vectors (Vector2, Vector3) and Color as arrays:\n" +
    "  - Vector2: [x, y]\n" +
    "  - Vector3: [x, y, z]\n" +
    "  - Color: [r, g, b, a]\n" +
    "- Do NOT modify the UID or component names.\n" +
    "- If nothing needs to change, respond with an empty JSON object: `{}`\n\n" +
    "You may also include a summary of what you changed in your Response.\n\n" +
    "Example input:\n" +
    "(scene JSON excerpt)\n" +
    "{\n" +
    "  \"0\": {\n" +
    "    \"uid\": \"0\",\n" +
    "    \"name\": \"Global Light 2D\",\n" +
    "    \"components\": [ ... ]\n" +
    "    ...\n" +
    "  }\n" +
    "}\n\n" +
    "User prompt:\n" +
    "\"Increase the light intensity and move the light up by 2 units.\"\n\n" +
    "Your output:\n" +
    "I have moved to light up 2 Units and increased the light intensity to 2.\n\n" +
    "```json\n" +
    "{\n" +
    "  \"0\": {\n" +
    "    \"Light2D\": {\n" +
    "      \"intensity\": 2.0\n" +
    "    },\n" +
    "    \"Transform\": {\n" +
    "      \"position\": [0.0, 2.0, 0.0]\n" +
    "    }\n" +
    "  }\n" +
    "}\n" +
    "```\n\n" +
    "Always respond with valid JSON enclosed in a fenced code block (starting with ```json).";

    public string Model { get; set; }
    private readonly string _key;

    public IEnumerator GetChatCompletion(AIMessage[] history, Action<string> callback)
    {
        var body = new AIRequest
        {
            model = Model,
            messages = history.Prepend(new AIMessage
            {
                role = "system",
                content = SystemMessage
            }).ToArray(),
            stream = false
        };

        var json = JsonConvert.SerializeObject(body);
        yield return WebRequestUtility.SendPostRequest(Endpoint, json, new Dictionary<string, string> {
            ["Content-Type"] = "application/json",
            ["Authorization"] = "Bearer " + _key
        }, request => OnRequestSuccess(request, callback), error => OnRequestError(error, callback));
    }

    
    private void OnRequestSuccess(UnityWebRequest request, Action<string> callback)
    {
        var response = JsonConvert.DeserializeObject<GroqResponse>(request.downloadHandler.text);
        callback(response.Choices.FirstOrDefault()?.Message.Content ?? "No response from AI.");
    }
    private void OnRequestError(string error, Action<string> callback)
    {
        Debug.LogError($"Error sending message: {error}");
        callback($"Error: {error}");
    }
}