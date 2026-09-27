using Demo1.DTOs;

namespace Demo1.Services
{
    public interface IDataService
    {
        // Dapatkan data dengan kaedah offset paging
        Task<OffsetPagingResponseDto<UserQuestionLogDTO>>
            GetByOffsetAsync(
                int page,
                int pageSize);

        // Dapatkan data dengan kaedah keyset paging (berdasarkan lastId)
        Task<KeysetPagingResponseDto<UserQuestionLogDTO>>
            GetByLastIdAsync(
                int? lastId,
                int pageSize);

        // Dapatkan satu rekod berdasarkan id
        Task<UserQuestionLogDTO?> GetByIdAsync(int id);
    }
}
