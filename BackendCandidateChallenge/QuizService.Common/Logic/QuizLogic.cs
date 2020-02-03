using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuizService.Common.Models;
using QuizService.Common.Repositories;

namespace QuizService.Common.Logic
{
    public class QuizLogic
    {
        private readonly QuizRepository _quizRepository;

        public QuizLogic(QuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }        
        
        public virtual async Task<IEnumerable<Quiz>> GetAll()
        {
            var quizzes = await _quizRepository.GetAll();

            return quizzes;
        }

        public virtual async Task<Quiz> GetById(int id)
        {
            var quiz = await _quizRepository.GetById(id);

            return quiz;
        }        
        
        
        
        
        
    }
}