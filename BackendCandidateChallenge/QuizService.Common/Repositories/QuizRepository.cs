using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using QuizService.Common.Models;

namespace QuizService.Common.Repositories
{
    public class QuizRepository
    {
        private readonly IDbConnection _connection;
        
        public QuizRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        internal virtual async Task<IEnumerable<Quiz>> GetAll()
        {
            const string query = @"
                SELECT [Id], [Title]
                FROM [Quiz]
            ";

            var quizzes = await _connection.QueryAsync<Quiz>(query);

            return quizzes;
        }

        internal virtual async Task<Quiz> GetById(int id)
        {
            const string query = @"
                SELECT [Id], [Title]
                FROM [Quiz]
                WHERE [Id] = @Id
            ";

            var quiz = await _connection.QuerySingleOrDefaultAsync<Quiz>(query, new {id});

            return quiz;
        }



    }
}