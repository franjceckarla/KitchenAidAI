using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/users")]
    public class UsersApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UsersApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] string? search,
            [FromQuery] string? username,
            [FromQuery] string? email,
            [FromQuery] string? zemlja,
            [FromQuery] PreferencijaPrehrane? preferencijaPrehrane,
            [FromQuery] bool? isDeleted,
            [FromQuery] bool? hasUploadedFiles,
            [FromQuery] DateTime? odDatuma,
            [FromQuery] DateTime? doDatuma,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDir,
            [FromQuery] bool includeAdmins = false,
            [FromQuery] bool? includeDeleted = null)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var usersQuery = _dbContext.Users
                .Include(user => user.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .Include(user => user.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .ThenInclude(join => join.recept)
                .AsQueryable();

            if (isAdmin)
            {
                if (!includeAdmins)
                {
                    usersQuery = usersQuery.Where(user => !user.isAdmin);
                }

                if (includeDeleted.HasValue && !includeDeleted.Value)
                {
                    usersQuery = usersQuery.Where(user => !user.isDeleted);
                }

                if (isDeleted.HasValue)
                {
                    usersQuery = usersQuery.Where(user => user.isDeleted == isDeleted.Value);
                }

                if (hasUploadedFiles.HasValue)
                {
                    usersQuery = hasUploadedFiles.Value
                        ? usersQuery.Where(user => _dbContext.Datoteke.Any(file => file.userId == user.id && !file.isDeleted))
                        : usersQuery.Where(user => !_dbContext.Datoteke.Any(file => file.userId == user.id && !file.isDeleted));
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    usersQuery = usersQuery.Where(user =>
                        (user.username ?? string.Empty).Contains(search)
                        || (user.email ?? string.Empty).Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(username))
                {
                    usersQuery = usersQuery.Where(user => (user.username ?? string.Empty).Contains(username));
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    usersQuery = usersQuery.Where(user => (user.email ?? string.Empty).Contains(email));
                }

                if (!string.IsNullOrWhiteSpace(zemlja))
                {
                    usersQuery = usersQuery.Where(user => (user.zemlja ?? string.Empty).Contains(zemlja));
                }

                if (preferencijaPrehrane.HasValue)
                {
                    usersQuery = usersQuery.Where(user => user.preferencijaPrehrane == preferencijaPrehrane.Value);
                }

                if (odDatuma.HasValue)
                {
                    var fromDate = odDatuma.Value.Date;
                    usersQuery = usersQuery.Where(user => user.kreirano >= fromDate);
                }

                if (doDatuma.HasValue)
                {
                    var toDate = doDatuma.Value.Date;
                    usersQuery = usersQuery.Where(user => user.kreirano <= toDate);
                }

                var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
                usersQuery = sortBy switch
                {
                    "username" => sortDescending
                        ? usersQuery.OrderByDescending(user => user.username)
                        : usersQuery.OrderBy(user => user.username),
                    "email" => sortDescending
                        ? usersQuery.OrderByDescending(user => user.email)
                        : usersQuery.OrderBy(user => user.email),
                    "kreirano" => sortDescending
                        ? usersQuery.OrderByDescending(user => user.kreirano)
                        : usersQuery.OrderBy(user => user.kreirano),
                    _ => usersQuery.OrderBy(user => user.id)
                };
            }
            else
            {
                usersQuery = usersQuery.Where(user => user.id == userId && !user.isDeleted);
            }

            var users = usersQuery.AsNoTracking().ToList();
            if (isAdmin)
            {
                var adminDtos = users.Select(user => user.ToDto()).ToList();
                return Ok(ApiResponse<List<UserDto>>.Ok(adminDtos));
            }

            var publicDtos = users.Select(user => user.ToPublicDto()).ToList();
            return Ok(ApiResponse<List<UserPublicDto>>.Ok(publicDtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin && userId != id)
            {
                return ApiForbidden("Nemate pravo pristupa ovom korisniku.");
            }

            var user = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .Include(currentUser => currentUser.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .ThenInclude(join => join.recept)
                .AsNoTracking()
                .FirstOrDefault(currentUser => currentUser.id == id && (isAdmin || !currentUser.isDeleted));
            if (user is null)
            {
                return ApiNotFound("Korisnik nije pronadjen.");
            }

            if (isAdmin)
            {
                return Ok(ApiResponse<UserDto>.Ok(user.ToDto()));
            }

            return Ok(ApiResponse<UserPublicDto>.Ok(user.ToPublicDto()));
        }

        [HttpPost]
        public IActionResult Create([FromBody] UserCreateDto input)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin)
            {
                return ApiForbidden("Samo admin moze kreirati korisnike.");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequest("Provjerite unesene podatke.");
            }

            var usernameExists = _dbContext.Users.Any(user => user.username != null
                && input.username != null
                && user.username.ToLower() == input.username.ToLower());
            if (usernameExists)
            {
                return ApiBadRequest("Korisnicko ime je vec zauzeto.", "USERNAME_EXISTS");
            }

            var emailExists = _dbContext.Users.Any(user => user.email != null
                && input.email != null
                && user.email.ToLower() == input.email.ToLower());
            if (emailExists)
            {
                return ApiBadRequest("Email je vec registriran.", "EMAIL_EXISTS");
            }

            var newUser = new User
            {
                username = input.username,
                ime = input.ime,
                prezime = input.prezime,
                datumRodenja = input.datumRodenja?.Date,
                zemlja = input.zemlja,
                email = input.email,
                preferencijaPrehrane = input.preferencijaPrehrane,
                isAdmin = false,
                frizider = new Frizider(),
                kuharica = new Kuharica { naziv = string.IsNullOrWhiteSpace(input.ime) ? "Kuharica" : $"{input.ime} kuharica" }
            };

            newUser.passwordHash = _passwordHasher.HashPassword(newUser, input.password ?? string.Empty);
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = newUser.id }, ApiResponse<UserDto>.Ok(newUser.ToDto()));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] UserUpdateDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin && userId != id)
            {
                return ApiForbidden("Nemate pravo izmjene ovog korisnika.");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequest("Provjerite unesene podatke.");
            }

            var user = _dbContext.Users.FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return ApiNotFound("Korisnik nije pronadjen.");
            }

            if (!isAdmin && user.isDeleted)
            {
                return ApiNotFound("Korisnik nije pronadjen.");
            }

            var usernameChanged = !string.Equals(user.username, input.username, StringComparison.Ordinal);
            user.username = input.username;
            user.email = input.email;
            user.preferencijaPrehrane = input.preferencijaPrehrane;

            if (isAdmin)
            {
                user.isAdmin = input.isAdmin;
                if (!string.IsNullOrWhiteSpace(input.newPassword))
                {
                    user.passwordHash = _passwordHasher.HashPassword(user, input.newPassword);
                }
            }

            if (usernameChanged && string.IsNullOrWhiteSpace(input.newPassword))
            {
                user.passwordHash = _passwordHasher.HashPassword(user, user.username ?? string.Empty);
            }

            _dbContext.SaveChanges();

            if (isAdmin)
            {
                return Ok(ApiResponse<UserDto>.Ok(user.ToDto()));
            }

            return Ok(ApiResponse<UserPublicDto>.Ok(user.ToPublicDto()));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin)
            {
                return ApiForbidden("Samo admin moze brisati korisnike.");
            }

            var fullUser = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .Include(currentUser => currentUser.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .Include(currentUser => currentUser.chatPoruke)
                .FirstOrDefault(currentUser => currentUser.id == id);
            if (fullUser is null)
            {
                return ApiNotFound("Korisnik nije pronadjen.");
            }

            fullUser.isDeleted = true;
            if (fullUser.frizider is not null)
            {
                fullUser.frizider.isDeleted = true;
                foreach (var item in fullUser.frizider.namirnice)
                {
                    item.isDeleted = true;
                }
            }

            if (fullUser.kuharica is not null)
            {
                fullUser.kuharica.isDeleted = true;
                foreach (var join in fullUser.kuharica.receptKuharice)
                {
                    join.isDeleted = true;
                }
            }

            foreach (var message in fullUser.chatPoruke)
            {
                message.isDeleted = true;
            }

            _dbContext.SaveChanges();

            return Ok(ApiResponse<UserDto>.Ok(fullUser.ToDto()));
        }

        [HttpPost("{id:int}/restore")]
        public IActionResult Restore(int id)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin)
            {
                return ApiForbidden("Samo admin moze vratiti korisnika.");
            }

            var user = _dbContext.Users
                .Include(currentUser => currentUser.frizider)
                .ThenInclude(fridge => fridge!.namirnice)
                .Include(currentUser => currentUser.kuharica)
                .ThenInclude(cookbook => cookbook!.receptKuharice)
                .Include(currentUser => currentUser.chatPoruke)
                .FirstOrDefault(currentUser => currentUser.id == id);
            if (user is null)
            {
                return ApiNotFound("Korisnik nije pronadjen.");
            }

            user.isDeleted = false;

            if (user.frizider is not null)
            {
                user.frizider.isDeleted = false;
                foreach (var item in user.frizider.namirnice)
                {
                    item.isDeleted = false;
                }
            }

            if (user.kuharica is not null)
            {
                user.kuharica.isDeleted = false;
                foreach (var join in user.kuharica.receptKuharice)
                {
                    join.isDeleted = false;
                }
            }

            foreach (var message in user.chatPoruke)
            {
                message.isDeleted = false;
            }

            _dbContext.SaveChanges();

            return Ok(ApiResponse<UserDto>.Ok(user.ToDto()));
        }
    }
}
