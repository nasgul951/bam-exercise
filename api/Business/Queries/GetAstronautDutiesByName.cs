using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Exceptions;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {

            var result = new GetAstronautDutiesByNameResult();

            var person = await _context.People
                .Where(z => z.Name == request.Name)
                .Select(p => new PersonAstronaut
                {
                    PersonId = p.Id,
                    Name = p.Name,
                    CurrentRank = p.AstronautDetail!.CurrentRank,
                    CurrentDutyTitle = p.AstronautDetail!.CurrentDutyTitle,
                    CareerStartDate = p.AstronautDetail!.CareerStartDate,
                    CareerEndDate = p.AstronautDetail.CareerEndDate
                }).FirstOrDefaultAsync();

            if (person is null) 
                throw new NotFoundException("No such person");

            result.Person = person;

            var duties = await _context.AstronautDuties
                .Where(d => d.PersonId == person.PersonId)
                .OrderByDescending(d => d.DutyStartDate)
                .ToListAsync();

            result.AstronautDuties = duties.ToList();

            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; } = new PersonAstronaut();
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
