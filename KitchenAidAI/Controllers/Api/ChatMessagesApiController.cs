using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/chat-messages")]
    public class ChatMessagesApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public ChatMessagesApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int? userId,
            [FromQuery] string? search,
            [FromQuery] TipOdgovora? tip,
            [FromQuery] DateTime? odDatuma,
            [FromQuery] DateTime? doDatuma,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDir,
            [FromQuery] bool includeDeleted = false)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var query = _dbContext.ChatMessages.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(message => message.userId == userId.Value);
            }

            if (isAdmin)
            {
                if (!includeDeleted)
                {
                    query = query.Where(message => !message.isDeleted);
                }
            }
            else
            {
                query = query.Where(message => message.userId == currentUserId && !message.isDeleted);
            }

            if (tip.HasValue)
            {
                query = query.Where(message => message.tip == tip.Value);
            }

            if (odDatuma.HasValue)
            {
                var fromDate = odDatuma.Value.Date;
                query = query.Where(message => message.kreirano >= fromDate);
            }

            if (doDatuma.HasValue)
            {
                var toDate = doDatuma.Value.Date;
                query = query.Where(message => message.kreirano <= toDate);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(message =>
                    (message.message ?? string.Empty).Contains(search)
                    || (message.response ?? string.Empty).Contains(search));
            }

            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy switch
            {
                "kreirano" => sortDescending
                    ? query.OrderByDescending(message => message.kreirano)
                    : query.OrderBy(message => message.kreirano),
                "tip" => sortDescending
                    ? query.OrderByDescending(message => message.tip)
                    : query.OrderBy(message => message.tip),
                _ => query.OrderBy(message => message.id)
            };

            var messages = query.AsNoTracking().ToList();
            if (isAdmin)
            {
                var adminDtos = messages.Select(message => new ChatMessageDto
                {
                    id = message.id,
                    userId = message.userId,
                    message = message.message,
                    response = message.response,
                    kreirano = message.kreirano,
                    tip = message.tip,
                    isDeleted = message.isDeleted
                }).ToList();
                return Ok(ApiResponse<List<ChatMessageDto>>.Ok(adminDtos));
            }

            var publicDtos = messages.Select(message => new ChatMessagePublicDto
            {
                id = message.id,
                userId = message.userId,
                message = message.message,
                response = message.response,
                kreirano = message.kreirano,
                tip = message.tip
            }).ToList();
            return Ok(ApiResponse<List<ChatMessagePublicDto>>.Ok(publicDtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var message = _dbContext.ChatMessages.AsNoTracking().FirstOrDefault(currentMessage => currentMessage.id == id && (isAdmin || !currentMessage.isDeleted));
            if (message is null)
            {
                return ApiNotFound("Poruka nije pronadjena.");
            }

            if (!isAdmin && message.userId != currentUserId)
            {
                return ApiForbidden("Nemate pravo pristupa ovoj poruci.");
            }

            if (isAdmin)
            {
                return Ok(ApiResponse<ChatMessageDto>.Ok(new ChatMessageDto
                {
                    id = message.id,
                    userId = message.userId,
                    message = message.message,
                    response = message.response,
                    kreirano = message.kreirano,
                    tip = message.tip,
                    isDeleted = message.isDeleted
                }));
            }

            return Ok(ApiResponse<ChatMessagePublicDto>.Ok(new ChatMessagePublicDto
            {
                id = message.id,
                userId = message.userId,
                message = message.message,
                response = message.response,
                kreirano = message.kreirano,
                tip = message.tip
            }));
        }

        [HttpPost]
        public IActionResult Create([FromBody] ChatMessageFormDto input)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var targetUserId = isAdmin ? input.userId : currentUserId;
            if (!isAdmin && input.userId != 0 && input.userId != currentUserId)
            {
                return ApiForbidden("Nemate pravo dodati poruku za drugog korisnika.");
            }

            var message = new ChatMessage
            {
                userId = targetUserId,
                message = input.message,
                response = input.response,
                tip = TipOdgovora.USER,
                isDeleted = false,
                kreirano = DateTime.Now
            };

            _dbContext.ChatMessages.Add(message);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = message.id }, ApiResponse<ChatMessageDto>.Ok(new ChatMessageDto
            {
                id = message.id,
                userId = message.userId,
                message = message.message,
                response = message.response,
                kreirano = message.kreirano,
                tip = message.tip,
                isDeleted = message.isDeleted
            }));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ChatMessageFormDto input)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var message = _dbContext.ChatMessages.FirstOrDefault(currentMessage => currentMessage.id == id && (isAdmin || !currentMessage.isDeleted));
            if (message is null)
            {
                return ApiNotFound("Poruka nije pronadjena.");
            }

            if (!isAdmin && message.userId != currentUserId)
            {
                return ApiForbidden("Nemate pravo uredjivati ovu poruku.");
            }

            message.message = input.message;
            if (isAdmin)
            {
                message.response = input.response;
            }

            _dbContext.SaveChanges();

            return Ok(ApiResponse<ChatMessageDto>.Ok(new ChatMessageDto
            {
                id = message.id,
                userId = message.userId,
                message = message.message,
                response = message.response,
                kreirano = message.kreirano,
                tip = message.tip,
                isDeleted = message.isDeleted
            }));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!TryGetSession(out var currentUserId, out var isAdmin, out var error))
            {
                return error!;
            }

            var message = _dbContext.ChatMessages.FirstOrDefault(currentMessage => currentMessage.id == id && (isAdmin || !currentMessage.isDeleted));
            if (message is null)
            {
                return ApiNotFound("Poruka nije pronadjena.");
            }

            if (!isAdmin && message.userId != currentUserId)
            {
                return ApiForbidden("Nemate pravo obrisati ovu poruku.");
            }

            message.isDeleted = true;
            _dbContext.SaveChanges();

            return Ok(ApiResponse<ChatMessageDto>.Ok(new ChatMessageDto
            {
                id = message.id,
                userId = message.userId,
                message = message.message,
                response = message.response,
                kreirano = message.kreirano,
                tip = message.tip,
                isDeleted = message.isDeleted
            }));
        }
    }
}
