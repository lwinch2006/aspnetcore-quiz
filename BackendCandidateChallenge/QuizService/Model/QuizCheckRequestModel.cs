using System.Collections.Generic;

namespace QuizService.Model
{
    public class QuizCheckRequestModel
    {
        public IEnumerable<QuestionAnswerPair> QuestionsWithAnswers { get; set; }
    }

    public class QuestionAnswerPair
    {
        public int QuestionId { get; set; }

        public int AnswerId { get; set; }
    }
}