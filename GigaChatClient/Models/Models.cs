using System.Text.Json.Serialization;
class Token
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }
}
public class Datum
{
    public string id { get; set; }
    public string @object { get; set; }
    public string owned_by { get; set; }
}
public class GigaChatModels
{
    public string @object { get; set; }
    public List<Datum> data { get; set; }
}
public class Message
{
    public string role { get; set; }  // system, user, assistant, function
    public string content { get; set; }
}
public class ChatRequest
{
    public string model { get; set; }  //GigaChat, GigaChat:latest, GigaChat-Plus, GigaChat-Pro
    public List<Message> messages { get; set; }

    public int temperature { get; set; } = 1;
    public double top_p { get; set; } = 0.1;
    public int n { get; set; } = 1;
    public bool stream { get; set; } = false;
    public int max_tokens { get; set; } = 512;
    public int repetition_penalty { get; set; } = 1;
    public int update_interval { get; set; } = 0;




}
public class Choice
{
    public Message message { get; set; }
    public int index { get; set; }
    public string finish_reason { get; set; }
}
public class GigaChatResult
{
    public List<Choice> choices { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public string @object { get; set; }
    public Usage usage { get; set; }
}
public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}
