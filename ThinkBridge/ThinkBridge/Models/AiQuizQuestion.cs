using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace ThinkBridge.Models
{
    public class AiQuizQuestion
    {
        public string question { get; set; }

        public string optionA { get; set; }

        public string optionB { get; set; }

        public string optionC { get; set; }

        public string optionD { get; set; }

        public string correctAnswer { get; set; }
    }
}