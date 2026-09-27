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

        [HttpGet("offset")]
        [AllowAnonymous]
        public async Task<
            ActionResult<
                OffsetPagingResponseDto<UserQuestionLogDTO>>>
            GetByOffset(
                int page = 1,
                int pageSize = 20)
        {
            var result =
                await _dataService
                    .GetByOffsetAsync(
                        page,
                        pageSize);

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