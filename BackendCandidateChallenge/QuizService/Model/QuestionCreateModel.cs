namespace QuizService.Model
{
    public class QuestionCreateModel
    {
        public QuestionCreateModel()
        {
        }
        
        public QuestionCreateModel(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}
