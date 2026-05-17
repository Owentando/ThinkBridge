using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThinkBridge.Services
{
    public class OpenTurnerAiService
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;

        public OpenTurnerAiService()
        {
            _apiKey = ConfigurationManager.AppSettings["OpenTurnerApiKey"];
            _model = ConfigurationManager.AppSettings["OpenTurnerModel"] ?? "liquid/lfm-2.5-1.2b-instruct:free";
            _apiUrl = ConfigurationManager.AppSettings["OpenTurnerApiUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://thinkbridge.edu");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "ThinkBridge Learning Assistant");
        }

        public async Task<string> GetAiResponseAsync(string prompt, string systemMessage = null)
        {
            try
            {
                var messages = new System.Collections.Generic.List<object>();

                if (!string.IsNullOrEmpty(systemMessage))
                    messages.Add(new { role = "system", content = systemMessage });

                messages.Add(new { role = "user", content = prompt });

                var requestBody = new
                {
                    model = _model,
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 2048
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"AI Service Error: {response.StatusCode}. Please check your API key.";

                var responseObj = JObject.Parse(responseString);
                var aiResponse = responseObj["choices"]?[0]?["message"]?.Value<string>("content");

                return aiResponse ?? "No response received from AI.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> GenerateVideoNotesAsync(string videoTitle, string description, string topic)
        {
            var systemMsg = "You are an educational note-taking assistant. Generate structured notes from video information.";
            var prompt = $"Generate comprehensive study notes based on this video lesson:\n\nTitle: {videoTitle}\nDescription: {description}\nTopic: {topic}\n\nCreate well-structured notes with key concepts, definitions, and examples. Use bullet points and clear headings.";
            return await GetAiResponseAsync(prompt, systemMsg);
        }
        public async Task<string> GenerateQuizQuestionsAsync(string notes, int numQuestions = 5)
        {
            var systemMsg = @"
You are an expert educational quiz generator.

Your task:
- Generate HIGH QUALITY academic multiple choice questions.
- Questions must test UNDERSTANDING, not keyword matching.
- Questions must come directly from the notes.
- Make distractors believable.
- Avoid vague questions.
- NEVER generate:
  - 'Random example'
  - 'Unrelated concept'
  - 'None of the above'

RULES:
- Each question must have:
  - question
  - optionA
  - optionB
  - optionC
  - optionD
  - correctAnswer

- correctAnswer MUST be A, B, C, or D only.

RETURN STRICT JSON ONLY.

FORMAT:
[
  {
    ""question"": ""..."",
    ""optionA"": ""..."",
    ""optionB"": ""..."",
    ""optionC"": ""..."",
    ""optionD"": ""..."",
    ""correctAnswer"": ""B""
  }
]
";

            var prompt = $@"
Generate {numQuestions} multiple choice questions from these study notes.

STUDY NOTES:
{notes}
";

            return await GetAiResponseAsync(prompt, systemMsg);
        }
        public async Task<string> ExplainWeakAreaAsync(string topic, string question)
        {
            var systemMsg = "You are a patient tutor. Explain concepts simply and clearly.";
            var prompt = $"A student is struggling with this topic: {topic}\n\nQuestion they got wrong: {question}\n\nPlease explain this concept in a simple, clear way suitable for a student. Include examples.";
            return await GetAiResponseAsync(prompt, systemMsg);
        }
        public async Task<string> ChatAsync(string message, string subjectContext = null, string topicContext = null)
        {
            var systemMsg = "You are ThinkBridge AI Tutor, a helpful learning assistant. Answer student questions clearly and concisely. Use simple language and provide examples when helpful.";

            if (!string.IsNullOrEmpty(subjectContext))
                systemMsg += $" The student is asking about {subjectContext}.";
            if (!string.IsNullOrEmpty(topicContext))
                systemMsg += $" Specifically about {topicContext}.";

            return await GetAiResponseAsync(message, systemMsg);
        }
        public async Task<string> SummarizeTextAsync(string text)
        {
            var systemMsg = "You are a helpful academic assistant. Summarize the following text clearly and concisely for a student. Use bullet points where appropriate. Focus on key concepts, definitions, and important details.";
            var prompt = $"Please summarize this educational content:\n\n{text}\n\nProvide a clear summary with key points. Use markdown-style formatting with headings and bullet points.";
            return await GetAiResponseAsync(prompt, systemMsg);
        }
        public async Task<string> GenerateStudyPlanAsync(string subject, string topics, DateTime testDate, int desiredMark, string weakAreas)
        {
            var systemMsg = "You are a smart study planner. Create a personalized, detailed study schedule. Use markdown-style formatting with clear headings, bullet points, and day-by-day breakdowns.";
            var prompt = $"Create a detailed study plan for {subject}.\n\n" +
                         $"Topics to cover: {topics}\n" +
                         $"Test date: {testDate:yyyy-MM-dd}\n" +
                         $"Desired mark: {desiredMark}%\n" +
                         $"Student context: {weakAreas}\n\n" +
                         $"Generate a day-by-day schedule from today until the test date. Include:\n" +
                         $"- Daily study tasks with specific topics\n" +
                         $"- Recommended practice activities\n" +
                         $"- Revision sessions for weak areas\n" +
                         $"- Break and review recommendations\n" +
                         $"- Final day preparation tips";

            return await GetAiResponseAsync(prompt, systemMsg);
        }
        public async Task<string> GenerateTopicsFromNotesAsync(string notes)
        {
            var systemMsg = "You are an academic assistant. Extract key topics from study notes.";

            var prompt = $@"
From the following notes, extract the main learning topics.

Rules:
- Return ONLY a list
- Each topic on a new line
- Maximum 6 topics
- Keep topics short (1–3 words)

Notes:
{notes}
";

            return await GetAiResponseAsync(prompt, systemMsg);
        }
    }
}