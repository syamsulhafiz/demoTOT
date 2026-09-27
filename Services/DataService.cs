using Demo1.Data;
using Demo1.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Services
{
    public class DataService : IDataService
    {
        private readonly TestDbContext _context;

        public DataService(TestDbContext context)
        {
            _context = context;
        }

        public async Task<UserQuestionLogDTO?> GetByIdAsync(int id)
        {
            var entity =
                await _context.UserQuestionLogs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return null;
            return new UserQuestionLogDTO
            {
                Id = entity.Id,
                Question = entity.Question,
                MatchedAnswer = entity.MatchedAnswer,
                AskedAt = entity.AskedAt,
                Score = entity.Score
            };
        }

        public async Task<
            OffsetPagingResponseDto<UserQuestionLogDTO>>
            GetByOffsetAsync(
                int page,
                int pageSize)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 20;

            if (pageSize > 100)
                pageSize = 100;

            var totalRecords =
                await _context.UserQuestionLogs
                    .AsNoTracking()
                    .CountAsync();

            var data =
                await _context.UserQuestionLogs
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new UserQuestionLogDTO
                    {
                        Id = x.Id,
                        Question = x.Question,
                        MatchedAnswer = x.MatchedAnswer,
                        AskedAt = x.AskedAt,
                        Score = x.Score
                    })
                    .ToListAsync();

            return new OffsetPagingResponseDto<UserQuestionLogDTO>
            {
                Page = page,

                PageSize = pageSize,

                TotalRecords = totalRecords,

                TotalPages =
                    (int)Math.Ceiling(
                        totalRecords /
                        (double)pageSize),

                Data = data
            };
        }


        public async Task<
            KeysetPagingResponseDto<UserQuestionLogDTO>>
            GetByLastIdAsync(
                int? lastId,
                int pageSize)
        {
            if (pageSize < 1)
                pageSize = 20;

            if (pageSize > 100)
                pageSize = 100;

            var query =
                _context.UserQuestionLogs
                    .AsNoTracking()
                    .AsQueryable();

            if (lastId.HasValue)
            {
                query =
                    query.Where(
                        x => x.Id < lastId.Value);
            }

            var data =
                await query
                    .OrderByDescending(x => x.Id)
                    .Take(pageSize)
                    .Select(x => new UserQuestionLogDTO
                    {
                        Id = x.Id,
                        Question = x.Question,
                        MatchedAnswer = x.MatchedAnswer,
                        AskedAt = x.AskedAt,
                        Score = x.Score
                    })
                    .ToListAsync();

            var nextLastId =
                data.Count > 0
                    ? data.Last().Id
                    : (int?)null;

            return new KeysetPagingResponseDto<UserQuestionLogDTO>
            {
                PageSize = pageSize,

                NextLastId = nextLastId,

                HasMore = data.Count == pageSize,

                Data = data
            };
        }
    }
}