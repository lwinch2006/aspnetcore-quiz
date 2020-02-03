using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using QuizService.Model;
using Xunit;

namespace QuizService.Tests
{
    public class QuizzesControllerTest
    {
        const string QuizApiEndPoint = "/api/quizzes/";

        [Fact]
        public async Task PostNewQuizAddsQuiz()
        {
            var quiz = new QuizCreateModel("Test title");
            using (var testHost = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()))
            {
                var client = testHost.CreateClient();
                var content = new StringContent(JsonConvert.SerializeObject(quiz));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),
                    content);
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(response.Headers.Location);
            }
        }

        [Fact]
        public async Task CheckQuiz_PostAnswers_AllAnswersCorrect_ShouldPass()
        {
            const int quizId = 1;
            
            // TODO (DKA): Make new quiz here with separate requests.

            var questionsWithAnswers = new QuizCheckRequestModel
            {
                QuestionsWithAnswers = new List<QuestionAnswerPair>
                {
                    new QuestionAnswerPair {QuestionId = 1, AnswerId = 1},
                    new QuestionAnswerPair {QuestionId = 2, AnswerId = 5}
                }
            };
            
            using (var testHost = new TestServer(new WebHostBuilder().UseStartup<Startup>()))
            {
                var client = testHost.CreateClient();
                var content = new StringContent(JsonConvert.SerializeObject(questionsWithAnswers));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                
                var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/check"), content);
                
                var quizResult = JsonConvert.DeserializeObject<QuizCheckResponseModel>(await response.Content.ReadAsStringAsync());
                
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(2, quizResult.TotalNumberOfAnswers);
                Assert.Equal(2, quizResult.NumberOfCorrectAnswers);
                Assert.Equal(quizResult.NumberOfCorrectAnswers, quizResult.TotalNumberOfAnswers);
            }
        }

        [Fact]
        public async Task CheckQuiz_PostAnswers_NotAllAnswersCorrect_ShouldPass()
        {
            const int quizId = 1;
            
            // TODO (DKA): Make new quiz here with separate requests.

            var questionsWithAnswers = new QuizCheckRequestModel
            {
                QuestionsWithAnswers = new List<QuestionAnswerPair>
                {
                    new QuestionAnswerPair {QuestionId = 1, AnswerId = 1},
                    new QuestionAnswerPair {QuestionId = 2, AnswerId = 4}
                }
            };
            
            using (var testHost = new TestServer(new WebHostBuilder().UseStartup<Startup>()))
            {
                var client = testHost.CreateClient();
                var content = new StringContent(JsonConvert.SerializeObject(questionsWithAnswers));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                
                var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/check"), content);
                
                var quizResult = JsonConvert.DeserializeObject<QuizCheckResponseModel>(await response.Content.ReadAsStringAsync());
                
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(2, quizResult.TotalNumberOfAnswers);
                Assert.Equal(1, quizResult.NumberOfCorrectAnswers);
                Assert.True(quizResult.NumberOfCorrectAnswers < quizResult.TotalNumberOfAnswers);
            }            
        }

        [Fact]
        public async Task AQuizExistGetReturnsQuiz()
        {
            using (var testHost = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()))
            {
                var client = testHost.CreateClient();
                const int quizId = 1;
                var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);
                var quiz = JsonConvert.DeserializeObject<QuizResponseModel>(await response.Content.ReadAsStringAsync());
                Assert.Equal(quizId, quiz.Id);
                Assert.Equal("My first quiz", quiz.Title);
            }
        }

        [Fact]
        public async Task AQuizDoesNotExistGetFails()
        {
            using (var testHost = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()))
            {
                var client = testHost.CreateClient();
                const long quizId = 999;
                var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        
        public async Task AQuizDoesNotExists_WhenPostingAQuestion_ReturnsNotFound()
        {
            const string QuizApiEndPoint = "/api/quizzes/999/questions";

            using (var testHost = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()))
            {
                var client = testHost.CreateClient();
                const long quizId = 999;
                var question = new QuestionCreateModel("The answer to everything is what?");
                var content = new StringContent(JsonConvert.SerializeObject(question));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),content);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
    }
}
