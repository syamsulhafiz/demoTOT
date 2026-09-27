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
            // Ambil rekod tunggal tanpa tracking untuk prestasi baca
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

        public async Task<OffsetPagingResponseDto<UserQuestionLogDTO>>
            GetByOffsetAsync(
                int page,
                int pageSize)
        {
            // Validasi asas input paging
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 20;

            if (pageSize > 100)
                pageSize = 100;

            // Kira jumlah rekod keseluruhan
            var totalRecords =
                await _context.UserQuestionLogs
                    .AsNoTracking()
                    .CountAsync();

            // Ambil data halaman semasa ikut turutan terbaru dahulu
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

            // Pulangkan respons paging lengkap
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
            // Validasi saiz halaman
            if (pageSize < 1)
                pageSize = 20;

            if (pageSize > 100)
                pageSize = 100;

            // Mulakan query asas
            var query =
                _context.UserQuestionLogs
                    .AsNoTracking()
                    .AsQueryable();

            // Jika lastId diberi, ambil rekod yang lebih lama (id lebih kecil)
            if (lastId.HasValue)
            {
                query =
                    query.Where(
                        x => x.Id < lastId.Value);
            }

            // Ambil satu batch data
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

            // Tentukan penanda (cursor) untuk batch seterusnya
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