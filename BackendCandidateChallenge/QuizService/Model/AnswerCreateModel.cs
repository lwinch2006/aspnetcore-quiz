namespace QuizService.Model
{
    public class AnswerCreateModel
    {
        public AnswerCreateModel(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}
