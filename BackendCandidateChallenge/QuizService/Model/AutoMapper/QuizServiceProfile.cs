using AutoMapper;

namespace QuizService.Model.AutoMapper
{
    public class QuizServiceProfile : Profile
    {
        public QuizServiceProfile()
        {
            // Logic model -> API contract.
            CreateMap<Common.Models.Quiz, QuizResponseModel>();
            
            // API contract -> Logic model.
            CreateMap<QuizResponseModel, Common.Models.Quiz>();            
        }
    }
}