using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using System.Linq;
using AutoMapper;
using QuizService.Common.Logic;
using QuizService.Common.Models;

namespace QuizService.Controllers
{
    // TODO (DKA):
    // As further refactoring of this (and other classies) I would:
    // - move all data access functionality to respective classes in Common.Repositories
    // - move all business logic functionality to respective classes in Common.Logic
    // - move all data models (business objects) to Common.Models.
    // - rename Model folder in QuizService to Contracts as:
    //   - API operates with Contracts
    //   - WebApp operates with ViewModels
    // - create separate solution folder for tests to gather them in one folder, dont blend with other projects
    // - clarify test names by UnitTest, IntegrationTests, SystemTests, etc. Not just Tests. 

    // - To learn more about my architecture meanings please see
    // https://github.com/lwinch2006/basic-aspnetcore-app
    
    [Route("api/quizzes")]
    public class QuizController : Controller
    {
        private readonly IDbConnection _connection;

        private readonly QuizLogic _quizLogic;

        private readonly IMapper _mapper;

        public QuizController(IDbConnection connection, QuizLogic quizLogic, IMapper mapper)
        {
            _connection = connection;
            _quizLogic = quizLogic;
            _mapper = mapper;
        }

        // GET api/quizzes
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var quizzesBO = await _quizLogic.GetAll();
            var quizzesContract = _mapper.Map<IEnumerable<QuizResponseModel>>(quizzesBO);
            return Ok(quizzesContract);
        }

        // GET api/quizzes/5
        [HttpGet("{id}")]
        public object Get(int id)
        {
            const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
            var quiz = _connection.QuerySingleOrDefault<Quiz>(quizSql, new {Id = id});
            if (quiz == null)
                return NotFound();
            const string questionsSql = "SELECT * FROM Question WHERE QuizId = @QuizId;";
            var questions = _connection.Query<Question>(questionsSql, new {QuizId = id});
            const string answersSql = "SELECT a.Id, a.Text, a.QuestionId FROM Answer a INNER JOIN Question q ON a.QuestionId = q.Id WHERE q.QuizId = @QuizId;";
            var answers = _connection.Query<Answer>(answersSql, new {QuizId = id})
                .Aggregate(new Dictionary<int, IList<Answer>>(), (dict, answer) => {
                    if (!dict.ContainsKey(answer.QuestionId))
                        dict.Add(answer.QuestionId, new List<Answer>());
                    dict[answer.QuestionId].Add(answer);
                    return dict;
                });
            return new QuizResponseModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Questions = questions.Select(question => new QuizResponseModel.QuestionItem
                {
                    Id = question.Id,
                    Text = question.Text,
                    Answers = answers.ContainsKey(question.Id)
                        ? answers[question.Id].Select(answer => new QuizResponseModel.AnswerItem
                        {
                            Id = answer.Id,
                            Text = answer.Text
                        })
                        : new QuizResponseModel.AnswerItem[0],
                    CorrectAnswerId = question.CorrectAnswerId
                }),
                Links = new Dictionary<string, string>
                {
                    {"self", $"/api/quizzes/{id}"},
                    {"questions", $"/api/quizzes/{id}/questions"}
                }
            };
        }

        // POST api/quizzes
        [HttpPost]
        public IActionResult Post([FromBody]QuizCreateModel value)
        {
            var sql = $"INSERT INTO Quiz (Title) VALUES('{value.Title}'); SELECT LAST_INSERT_ROWID();";
            var id = _connection.ExecuteScalar(sql);
            return Created($"/api/quizzes/{id}", null);
        }

        // PUT api/quizzes/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]QuizUpdateModel value)
        {
            const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
            int rowsUpdated = _connection.Execute(sql, new {Id = id, Title = value.Title});
            if (rowsUpdated == 0)
                return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/check")]
        public IActionResult CheckQuiz(int id, [FromBody] QuizCheckRequestModel questionsWithAnswers)
        {
            var numberOfCorrectAnswers = 0;

            if (questionsWithAnswers == null ||
                questionsWithAnswers.QuestionsWithAnswers == null ||
                !questionsWithAnswers.QuestionsWithAnswers.Any())
            {
                return BadRequest();
            }
            
            const string query = "SELECT * FROM Question WHERE QuizId = @Id;";
            
            var questions = _connection.Query<Question>(query, new {Id = id});
            if (questions == null)
            {
                return NotFound();
            }

            var totalNumberOfAnswers = questions.Count();
            
            foreach (var questionWithAnswer in questionsWithAnswers.QuestionsWithAnswers)
            {
                if (questions.Any(question => question.Id == questionWithAnswer.QuestionId
                                              && question.CorrectAnswerId == questionWithAnswer.AnswerId))
                {
                    numberOfCorrectAnswers++;
                }
            }
            
            return Ok(new QuizCheckResponseModel
            {
                NumberOfCorrectAnswers = numberOfCorrectAnswers,
                TotalNumberOfAnswers = totalNumberOfAnswers
            });
        }
        
        // DELETE api/quizzes/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            const string sql = "DELETE FROM Quiz WHERE Id = @Id";
            int rowsDeleted = _connection.Execute(sql, new {Id = id});
            if (rowsDeleted == 0)
                return NotFound();
            return NoContent();
        }

        // POST api/quizzes/5/questions
        [HttpPost]
        [Route("{id}/questions")]
        public IActionResult PostQuestion(int id, [FromBody]QuestionCreateModel value)
        {
            const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
            var quiz = _connection.QuerySingleOrDefault<Quiz>(quizSql, new {Id = id});
            if (quiz == null)
                return NotFound();
            
            const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";
            var questionId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuizId = id});
            return Created($"/api/quizzes/{id}/questions/{questionId}", null);
        }

        // PUT api/quizzes/5/questions/6
        [HttpPut("{id}/questions/{qid}")]
        public IActionResult PutQuestion(int id, int qid, [FromBody]QuestionUpdateModel value)
        {
            const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
            int rowsUpdated = _connection.Execute(sql, new {QuestionId = qid, Text = value.Text, CorrectAnswerId = value.CorrectAnswerId});
            if (rowsUpdated == 0)
                return NotFound();
            return NoContent();
        }

        // DELETE api/quizzes/5/questions/6
        [HttpDelete]
        [Route("{id}/questions/{qid}")]
        public IActionResult DeleteQuestion(int id, int qid)
        {
            const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
            _connection.ExecuteScalar(sql, new {QuestionId = qid});
            return NoContent();
        }

        // POST api/quizzes/5/questions/6/answers
        [HttpPost]
        [Route("{id}/questions/{qid}/answers")]
        public IActionResult PostAnswer(int id, int qid, [FromBody]AnswerCreateModel value)
        {
            const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
            var answerId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuestionId = qid});
            return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", null);
        }

        // PUT api/quizzes/5/questions/6/answers/7
        [HttpPut("{id}/questions/{qid}/answers/{aid}")]
        public IActionResult PutAnswer(int id, int qid, int aid, [FromBody]AnswerUpdateModel value)
        {
            const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
            int rowsUpdated = _connection.Execute(sql, new {AnswerId = qid, Text = value.Text});
            if (rowsUpdated == 0)
                return NotFound();
            return NoContent();
        }

        // DELETE api/quizzes/5/questions/6/answers/7
        [HttpDelete]
        [Route("{id}/questions/{qid}/answers/{aid}")]
        public IActionResult DeleteAnswer(int id, int qid, int aid)
        {
            const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
            _connection.ExecuteScalar(sql, new {AnswerId = aid});
            return NoContent();
        }
        
        
        
        
        
        
        
        
    }
}
