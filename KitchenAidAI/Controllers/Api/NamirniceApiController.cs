using KitchenAidAI.Data;
using KitchenAidAI.Models;
using KitchenAidAI.Models.DTOs;
using KitchenAidAI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenAidAI.Controllers.Api
{
    [Route("api/namirnice")]
    public class NamirniceApiController : ApiControllerBase
    {
        private readonly KitchenAidDbContext _dbContext;

        public NamirniceApiController(KitchenAidDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int? friziderId,
            [FromQuery] string? search,
            [FromQuery] KategorijaNamirnice? kategorija,
            [FromQuery] Mjera? mjera,
            [FromQuery] double? minKolicina,
            [FromQuery] double? maxKolicina,
            [FromQuery] double? minKalorije,
            [FromQuery] double? maxKalorije,
            [FromQuery] double? minProteini,
            [FromQuery] double? maxProteini,
            [FromQuery] double? minMasti,
            [FromQuery] double? maxMasti,
            [FromQuery] double? minUgljikohidrati,
            [FromQuery] double? maxUgljikohidrati,
            [FromQuery] double? minVlakna,
            [FromQuery] double? maxVlakna,
            [FromQuery] double? minSol,
            [FromQuery] double? maxSol,
            [FromQuery] DateTime? odDatuma,
            [FromQuery] DateTime? doDatuma,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDir,
            [FromQuery] bool includeDeleted = false)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var query = _dbContext.Namirnice
                .Include(item => item.frizider)
                .AsQueryable();

            if (friziderId.HasValue)
            {
                query = query.Where(item => item.friziderId == friziderId.Value);
            }

            if (kategorija.HasValue)
            {
                query = query.Where(item => item.kategorija == kategorija.Value);
            }

            if (mjera.HasValue)
            {
                query = query.Where(item => item.mjera == mjera.Value);
            }

            if (minKolicina.HasValue)
            {
                query = query.Where(item => item.kolicinaUFrizideru >= minKolicina.Value);
            }

            if (maxKolicina.HasValue)
            {
                query = query.Where(item => item.kolicinaUFrizideru <= maxKolicina.Value);
            }

            if (minKalorije.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.kalorije >= minKalorije.Value);
            }

            if (maxKalorije.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.kalorije <= maxKalorije.Value);
            }

            if (minProteini.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.proteini >= minProteini.Value);
            }

            if (maxProteini.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.proteini <= maxProteini.Value);
            }

            if (minMasti.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.masti >= minMasti.Value);
            }

            if (maxMasti.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.masti <= maxMasti.Value);
            }

            if (minUgljikohidrati.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.ugljikohidrati >= minUgljikohidrati.Value);
            }

            if (maxUgljikohidrati.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.ugljikohidrati <= maxUgljikohidrati.Value);
            }

            if (minVlakna.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.vlakna >= minVlakna.Value);
            }

            if (maxVlakna.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.vlakna <= maxVlakna.Value);
            }

            if (minSol.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.sol >= minSol.Value);
            }

            if (maxSol.HasValue)
            {
                query = query.Where(item => item.nutritivnaVrijednost != null
                    && item.nutritivnaVrijednost.sol <= maxSol.Value);
            }

            if (odDatuma.HasValue)
            {
                var fromDate = odDatuma.Value.Date;
                query = query.Where(item => item.kreirano >= fromDate);
            }

            if (doDatuma.HasValue)
            {
                var toDate = doDatuma.Value.Date;
                query = query.Where(item => item.kreirano <= toDate);
            }

            if (!isAdmin)
            {
                query = query.Where(item => item.frizider != null && item.frizider.userId == userId && !item.isDeleted);
            }
            else if (!includeDeleted)
            {
                query = query.Where(item => !item.isDeleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(item => (item.naziv ?? string.Empty).Contains(search));
            }

            var sortDescending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy switch
            {
                "naziv" => sortDescending
                    ? query.OrderByDescending(item => item.naziv)
                    : query.OrderBy(item => item.naziv),
                "kolicina" => sortDescending
                    ? query.OrderByDescending(item => item.kolicinaUFrizideru)
                    : query.OrderBy(item => item.kolicinaUFrizideru),
                "kreirano" => sortDescending
                    ? query.OrderByDescending(item => item.kreirano)
                    : query.OrderBy(item => item.kreirano),
                "kalorije" => sortDescending
                    ? query.OrderByDescending(item => item.nutritivnaVrijednost != null
                        ? item.nutritivnaVrijednost.kalorije
                        : 0)
                    : query.OrderBy(item => item.nutritivnaVrijednost != null
                        ? item.nutritivnaVrijednost.kalorije
                        : 0),
                _ => query.OrderBy(item => item.id)
            };

            var items = query.AsNoTracking().ToList();
            if (isAdmin)
            {
                var adminDtos = items.Select(item => item.ToDto()).ToList();
                return Ok(ApiResponse<List<NamirnicaDto>>.Ok(adminDtos));
            }

            var publicDtos = items.Select(item => item.ToPublicDto()).ToList();
            return Ok(ApiResponse<List<NamirnicaPublicDto>>.Ok(publicDtos));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var item = _dbContext.Namirnice
                .Include(currentItem => currentItem.frizider)
                .AsNoTracking()
                .FirstOrDefault(currentItem => currentItem.id == id && (isAdmin || !currentItem.isDeleted));
            if (item is null)
            {
                return ApiNotFound("Namirnica nije pronadjena.");
            }

            if (!isAdmin && item.frizider?.userId != userId)
            {
                return ApiForbidden("Nemate pravo pristupa ovoj namirnici.");
            }

            if (isAdmin)
            {
                return Ok(ApiResponse<NamirnicaDto>.Ok(item.ToDto()));
            }

            return Ok(ApiResponse<NamirnicaPublicDto>.Ok(item.ToPublicDto()));
        }

        [HttpPost]
        public IActionResult Create([FromBody] NamirnicaFormDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequest("Provjerite unesene podatke.");
            }

            var fridge = _dbContext.Frizideri.FirstOrDefault(currentFridge => currentFridge.id == input.friziderId && (isAdmin || !currentFridge.isDeleted));
            if (fridge is null)
            {
                return ApiNotFound("Frizider nije pronadjen.");
            }

            if (!isAdmin && fridge.userId != userId)
            {
                return ApiForbidden("Nemate pravo dodavati u ovaj frizider.");
            }

            var newItem = new Namirnica
            {
                friziderId = input.friziderId,
                naziv = input.naziv,
                kategorija = input.kategorija,
                mjera = input.mjera,
                kolicinaUFrizideru = input.kolicinaUFrizideru
            };

            _dbContext.Namirnice.Add(newItem);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = newItem.id }, ApiResponse<NamirnicaDto>.Ok(newItem.ToDto()));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] NamirnicaFormDto input)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequest("Provjerite unesene podatke.");
            }

            var item = _dbContext.Namirnice
                .Include(currentItem => currentItem.frizider)
                .FirstOrDefault(currentItem => currentItem.id == id && (isAdmin || !currentItem.isDeleted));
            if (item is null)
            {
                return ApiNotFound("Namirnica nije pronadjena.");
            }

            if (!isAdmin && item.frizider?.userId != userId)
            {
                return ApiForbidden("Nemate pravo uredjivati ovu namirnicu.");
            }

            item.naziv = input.naziv;
            item.kategorija = input.kategorija;
            item.mjera = input.mjera;
            item.kolicinaUFrizideru = input.kolicinaUFrizideru;
            item.friziderId = input.friziderId;

            _dbContext.SaveChanges();

            if (isAdmin)
            {
                return Ok(ApiResponse<NamirnicaDto>.Ok(item.ToDto()));
            }

            return Ok(ApiResponse<NamirnicaPublicDto>.Ok(item.ToPublicDto()));
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!TryGetSession(out var userId, out var isAdmin, out var error))
            {
                return error!;
            }

            var item = _dbContext.Namirnice
                .Include(currentItem => currentItem.frizider)
                .FirstOrDefault(currentItem => currentItem.id == id && (isAdmin || !currentItem.isDeleted));
            if (item is null)
            {
                return ApiNotFound("Namirnica nije pronadjena.");
            }

            if (!isAdmin && item.frizider?.userId != userId)
            {
                return ApiForbidden("Nemate pravo obrisati ovu namirnicu.");
            }

            item.isDeleted = true;
            _dbContext.SaveChanges();

            return Ok(ApiResponse<NamirnicaDto>.Ok(item.ToDto()));
        }
    }
}
