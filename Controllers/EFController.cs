using Demo1.DTOs;
using Demo1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EFController : ControllerBase
    {
        private readonly IDataService _dataService;

        public EFController(
            IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Sample endpoint tanpa authentication, untuk mendapatkan data berdasarkan ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserQuestionLogDTO>>> GetById(Int32 id)
        {
            var result =
                await _dataService
                    .GetByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Dapatkan senarai log soalan pengguna secara berhalaman (offset paging).
        /// </summary>
        /// <param name="page">Nombor halaman bermula dari 1.</param>
        /// <param name="pageSize">Bilangan rekod bagi setiap halaman.</param>
        /// <returns>Senarai log soalan pengguna.</returns>
        /// <response code="200">Permintaan berjaya.</response>
        /// <response code="400">Parameter tidak sah.</response>
        [HttpGet("offset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OffsetPagingResponseDto<UserQuestionLogDTO>>> GetByOffset(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _dataService.GetByOffsetAsync(page, pageSize);
            return Ok(result);
        }


        [HttpGet("keyset")]
        [AllowAnonymous]
        public async Task<
            ActionResult<
                KeysetPagingResponseDto<UserQuestionLogDTO>>>
            GetByLastId(
                int? lastId = null,
                int pageSize = 20)
        {
            var result =
                await _dataService
                    .GetByLastIdAsync(
                        lastId,
                        pageSize);

            return Ok(result);
        }
    }
}