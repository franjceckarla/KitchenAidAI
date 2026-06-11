using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/countries")]
    public class CountriesApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public CountriesApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDir)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            var query = _dbContext.Countries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(country => (country.naziv ?? string.Empty).Contains(search));
            }

            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy switch
            {
                "naziv" => sortDescending
                    ? query.OrderByDescending(country => country.naziv)
                    : query.OrderBy(country => country.naziv),
                _ => query.OrderBy(country => country.id)
            };

            var countries = query.AsNoTracking().ToList();
            var dtos = countries.Select(country => new CountryDto { id = country.id, naziv = country.naziv }).ToList();
            return Ok(ApiResponse<List<CountryDto>>.Ok(dtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            var country = _dbContext.Countries.AsNoTracking().FirstOrDefault(currentCountry => currentCountry.id == id);
            if (country is null)
            {
                return ApiNotFound("Drzava nije pronadjena.");
            }

            return Ok(ApiResponse<CountryDto>.Ok(new CountryDto { id = country.id, naziv = country.naziv }));
        }

        [HttpPost]
        public IActionResult Create([FromBody] CountryDto input)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin)
            {
                return ApiForbidden("Samo admin moze kreirati drzave.");
            }

            if (string.IsNullOrWhiteSpace(input.naziv))
            {
                return ApiBadRequest("Naziv drzave je obavezan.");
            }

            var exists = _dbContext.Countries.Any(country => country.naziv != null && country.naziv.ToLower() == input.naziv.ToLower());
            if (exists)
            {
                return ApiBadRequest("Drzava vec postoji.", "COUNTRY_EXISTS");
            }

            var country = new Country { naziv = input.naziv };
            _dbContext.Countries.Add(country);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = country.id }, ApiResponse<CountryDto>.Ok(new CountryDto { id = country.id, naziv = country.naziv }));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] CountryDto input)
        {
            if (!TryGetSession(out _, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!isAdmin)
            {
                return ApiForbidden("Samo admin moze uredjivati drzave.");
            }

            var country = _dbContext.Countries.FirstOrDefault(currentCountry => currentCountry.id == id);
            if (country is null)
            {
                return ApiNotFound("Drzava nije pronadjena.");
            }

            if (string.IsNullOrWhiteSpace(input.naziv))
            {
                return ApiBadRequest("Naziv drzave je obavezan.");
            }

            country.naziv = input.naziv;
            _dbContext.SaveChanges();

            return Ok(ApiResponse<CountryDto>.Ok(new CountryDto { id = country.id, naziv = country.naziv }));
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
                return ApiForbidden("Samo admin moze brisati drzave.");
            }

            var country = _dbContext.Countries.FirstOrDefault(currentCountry => currentCountry.id == id);
            if (country is null)
            {
                return ApiNotFound("Drzava nije pronadjena.");
            }

            _dbContext.Countries.Remove(country);
            _dbContext.SaveChanges();

            return Ok(ApiResponse<CountryDto>.Ok(new CountryDto { id = country.id, naziv = country.naziv }));
        }
    }
}
