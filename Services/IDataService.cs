using Demo1.DTOs;

namespace Demo1.Services
{
    public interface IDataService
    {
        Task<OffsetPagingResponseDto<UserQuestionLogDTO>>
            GetByOffsetAsync(
                int page,
                int pageSize);

        Task<KeysetPagingResponseDto<UserQuestionLogDTO>>
            GetByLastIdAsync(
                int? lastId,
                int pageSize);

        Task<UserQuestionLogDTO?> GetByIdAsync(int id);
    }
}
