using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.Courier.Models;
using Server.Modules.Courier.Services;

namespace Server.Modules.Courier.Controllers;

[ApiController]
[Route("api/courier")]
[Authorize]
public class CourierSettingsController : ControllerBase
{
    private readonly ICourierServiceTypeService _serviceTypeService;
    private readonly ICourierZoneService _zoneService;
    private readonly IZoneRateService _rateService;
    private readonly ICourierAgentService _agentService;
    private readonly ICourierSettingsService _courierSettingsService;
    private readonly ITenantProvider _tenantProvider;
    private readonly AppDbContext _context;

    public CourierSettingsController(
        ICourierServiceTypeService serviceTypeService,
        ICourierZoneService zoneService,
        IZoneRateService rateService,
        ICourierAgentService agentService,
        ICourierSettingsService courierSettingsService,
        ITenantProvider tenantProvider,
        AppDbContext context)
    {
        _serviceTypeService = serviceTypeService;
        _zoneService = zoneService;
        _rateService = rateService;
        _agentService = agentService;
        _courierSettingsService = courierSettingsService;
        _tenantProvider = tenantProvider;
        _context = context;
    }

    #region Control Account Settings
    [HttpGet("settings")]
    public async Task<ActionResult<CourierSettingsDto>> GetSettings()
    {
        var settings = await _courierSettingsService.GetWithAccountNamesAsync();
        if (settings == null)
        {
            return Ok(new CourierSettingsDto());
        }
        return Ok(settings);
    }

    [HttpPut("settings")]
    public async Task<ActionResult<CourierSettingsDto>> UpdateSettings([FromBody] CourierSettingsUpdateDto dto)
    {
        try
        {
            var updated = await _courierSettingsService.UpsertAsync(dto);
            var result = await _courierSettingsService.GetWithAccountNamesAsync();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    #endregion

    #region Service Types
    [HttpGet("service-types")]
    public async Task<ActionResult<List<CourierServiceTypeDto>>> GetServiceTypes()
    {
        var items = await _serviceTypeService.GetAllAsync();
        return Ok(items.Select(MapToServiceTypeDto));
    }

    [HttpGet("service-types/{id}")]
    public async Task<ActionResult<CourierServiceTypeDto>> GetServiceType(Guid id)
    {
        var item = await _serviceTypeService.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(MapToServiceTypeDto(item));
    }

    [HttpPost("service-types")]
    public async Task<ActionResult<CourierServiceTypeDto>> CreateServiceType([FromBody] CourierServiceTypeDto dto)
    {
        if (await _serviceTypeService.ExistsAsync(dto.Code))
            return BadRequest("A service type with this code already exists.");

        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required.");

        var entity = new CourierServiceType
        {
            TenantId = tenantId.Value,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            DefaultTransitDays = dto.DefaultTransitDays,
            AllowCOD = dto.AllowCOD,
            AllowInsurance = dto.AllowInsurance,
            IsExpress = dto.IsExpress,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive
        };

        var created = await _serviceTypeService.CreateAsync(entity);
        return CreatedAtAction(nameof(GetServiceType), new { id = created.Id }, MapToServiceTypeDto(created));
    }

    [HttpPut("service-types/{id}")]
    public async Task<ActionResult<CourierServiceTypeDto>> UpdateServiceType(Guid id, [FromBody] CourierServiceTypeDto dto)
    {
        var existing = await _serviceTypeService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        if (await _serviceTypeService.ExistsAsync(dto.Code, id))
            return BadRequest("A service type with this code already exists.");

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.DefaultTransitDays = dto.DefaultTransitDays;
        existing.AllowCOD = dto.AllowCOD;
        existing.AllowInsurance = dto.AllowInsurance;
        existing.IsExpress = dto.IsExpress;
        existing.SortOrder = dto.SortOrder;
        existing.IsActive = dto.IsActive;

        var updated = await _serviceTypeService.UpdateAsync(existing);
        return Ok(MapToServiceTypeDto(updated));
    }

    [HttpDelete("service-types/{id}")]
    public async Task<ActionResult> DeleteServiceType(Guid id)
    {
        var deleted = await _serviceTypeService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    private static CourierServiceTypeDto MapToServiceTypeDto(CourierServiceType entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Description = entity.Description,
        DefaultTransitDays = entity.DefaultTransitDays,
        AllowCOD = entity.AllowCOD,
        AllowInsurance = entity.AllowInsurance,
        IsExpress = entity.IsExpress,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };
    #endregion

    #region Zones
    [HttpGet("zones")]
    public async Task<ActionResult<List<CourierZoneDto>>> GetZones([FromQuery] string? categoryCode = null)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required.");

        IQueryable<CourierZone> query = _context.CourierZones
            .Include(z => z.ZoneCategory)
            .Include(z => z.ZoneCountries)
            .Include(z => z.ZoneStates)
            .Where(z => z.TenantId == tenantId.Value);

        if (!string.IsNullOrEmpty(categoryCode))
        {
            query = query.Where(z => z.ZoneCategory != null && z.ZoneCategory.Code == categoryCode);
        }

        var items = await query.OrderBy(z => z.ZoneName).ToListAsync();
        return Ok(items.Select(MapToZoneDto));
    }

    [HttpGet("zones/{id}")]
    public async Task<ActionResult<CourierZoneDto>> GetZone(Guid id)
    {
        var item = await _zoneService.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(MapToZoneDto(item));
    }

    [HttpPost("zones")]
    public async Task<ActionResult<CourierZoneDto>> CreateZone([FromBody] CourierZoneDto dto)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required.");

        if (!dto.ZoneCategoryId.HasValue)
            return BadRequest("Zone Category is required. Please select a zone category.");

        var categoryExists = await _context.ZoneCategories
            .AnyAsync(zc => zc.Id == dto.ZoneCategoryId && zc.TenantId == tenantId.Value);
        if (!categoryExists)
            return BadRequest("The specified Zone Category does not exist.");

        if (await _zoneService.ExistsAsync(dto.ZoneCategoryId.Value, dto.ZoneCode))
            return BadRequest("A zone with this code already exists for this carrier/category.");

        var zoneType = Enum.TryParse<ZoneType>(dto.ZoneType, out var zt) ? zt : ZoneType.Local;

        if (zoneType == ZoneType.International && (dto.Countries == null || !dto.Countries.Any()))
            return BadRequest("International zones require at least one country.");
        
        if (zoneType == ZoneType.Local && (dto.States == null || !dto.States.Any()))
            return BadRequest("Local zones require at least one state.");

        var entity = new CourierZone
        {
            TenantId = tenantId.Value,
            ZoneCode = dto.ZoneCode,
            ZoneName = dto.ZoneName,
            Description = dto.Description,
            ZoneCategoryId = dto.ZoneCategoryId,
            ZoneType = zoneType,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Countries != null && dto.Countries.Any())
        {
            foreach (var country in dto.Countries)
            {
                entity.ZoneCountries.Add(new CourierZoneCountry
                {
                    TenantId = tenantId.Value,
                    CountryId = country.CountryId,
                    CountryName = country.CountryName,
                    CountryCode = country.CountryCode,
                    SortOrder = country.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (dto.States != null && dto.States.Any())
        {
            foreach (var state in dto.States)
            {
                entity.ZoneStates.Add(new CourierZoneState
                {
                    TenantId = tenantId.Value,
                    StateId = state.StateId,
                    StateName = state.StateName,
                    StateCode = state.StateCode,
                    CountryId = state.CountryId,
                    CountryName = state.CountryName,
                    SortOrder = state.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var created = await _zoneService.CreateAsync(entity);
        return CreatedAtAction(nameof(GetZone), new { id = created.Id }, MapToZoneDto(created));
    }

    [HttpPut("zones/{id}")]
    public async Task<ActionResult<CourierZoneDto>> UpdateZone(Guid id, [FromBody] CourierZoneDto dto)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required.");

        var existing = await _zoneService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        if (!dto.ZoneCategoryId.HasValue)
            return BadRequest("Zone Category is required. Please select a zone category.");

        var categoryExists = await _context.ZoneCategories
            .AnyAsync(zc => zc.Id == dto.ZoneCategoryId && zc.TenantId == tenantId.Value);
        if (!categoryExists)
            return BadRequest("The specified Zone Category does not exist.");

        if (await _zoneService.ExistsAsync(dto.ZoneCategoryId.Value, dto.ZoneCode, id))
            return BadRequest("A zone with this code already exists for this carrier/category.");

        var zoneType = Enum.TryParse<ZoneType>(dto.ZoneType, out var zt) ? zt : ZoneType.Local;

        if (zoneType == ZoneType.International && (dto.Countries == null || !dto.Countries.Any()))
            return BadRequest("International zones require at least one country.");
        
        if (zoneType == ZoneType.Local && (dto.States == null || !dto.States.Any()))
            return BadRequest("Local zones require at least one state.");

        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"CourierZoneCountries\" WHERE \"CourierZoneId\" = {0}", id);
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"CourierZoneStates\" WHERE \"CourierZoneId\" = {0}", id);

        foreach (var entry in _context.ChangeTracker.Entries<CourierZoneCountry>().ToList())
            entry.State = EntityState.Detached;
        foreach (var entry in _context.ChangeTracker.Entries<CourierZoneState>().ToList())
            entry.State = EntityState.Detached;

        existing = await _zoneService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.ZoneName = dto.ZoneName;
        existing.Description = dto.Description;
        existing.ZoneCategoryId = dto.ZoneCategoryId;
        existing.ZoneType = zoneType;
        existing.IsActive = dto.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        if (existing.CreatedAt.Kind == DateTimeKind.Unspecified)
            existing.CreatedAt = DateTime.SpecifyKind(existing.CreatedAt, DateTimeKind.Utc);

        if (dto.Countries != null && dto.Countries.Any())
        {
            foreach (var country in dto.Countries)
            {
                existing.ZoneCountries.Add(new CourierZoneCountry
                {
                    TenantId = tenantId.Value,
                    CountryId = country.CountryId,
                    CountryName = country.CountryName,
                    CountryCode = country.CountryCode,
                    SortOrder = country.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (dto.States != null && dto.States.Any())
        {
            foreach (var state in dto.States)
            {
                existing.ZoneStates.Add(new CourierZoneState
                {
                    TenantId = tenantId.Value,
                    StateId = state.StateId,
                    StateName = state.StateName,
                    StateCode = state.StateCode,
                    CountryId = state.CountryId,
                    CountryName = state.CountryName,
                    SortOrder = state.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var updated = await _zoneService.UpdateAsync(existing);
        return Ok(MapToZoneDto(updated));
    }

    [HttpDelete("zones/{id}")]
    public async Task<ActionResult> DeleteZone(Guid id)
    {
        var deleted = await _zoneService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    [HttpPatch("zones/{id}/toggle-active")]
    public async Task<ActionResult<CourierZoneDto>> ToggleZoneActive(Guid id)
    {
        var existing = await _zoneService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.IsActive = !existing.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _zoneService.UpdateAsync(existing);
        return Ok(MapToZoneDto(updated));
    }

    private static CourierZoneDto MapToZoneDto(CourierZone entity) => new()
    {
        Id = entity.Id,
        ZoneCode = entity.ZoneCode,
        ZoneName = entity.ZoneName,
        Description = entity.Description,
        ZoneCategoryId = entity.ZoneCategoryId,
        ZoneCategoryName = entity.ZoneCategory?.Name,
        ZoneType = entity.ZoneType.ToString(),
        IsActive = entity.IsActive,
        Countries = entity.ZoneCountries.Select(c => new ZoneCountryDto
        {
            Id = c.Id,
            CountryId = c.CountryId,
            CountryName = c.CountryName,
            CountryCode = c.CountryCode,
            SortOrder = c.SortOrder
        }).OrderBy(c => c.SortOrder).ToList(),
        States = entity.ZoneStates.Select(s => new ZoneStateDto
        {
            Id = s.Id,
            StateId = s.StateId,
            StateName = s.StateName,
            StateCode = s.StateCode,
            CountryId = s.CountryId,
            CountryName = s.CountryName,
            SortOrder = s.SortOrder
        }).OrderBy(s => s.SortOrder).ToList()
    };
    #endregion

    #region Zone Rates
    [HttpGet("zone-rates")]
    public async Task<ActionResult<List<ZoneRateDto>>> GetZoneRates([FromQuery] Guid? zoneId, [FromQuery] Guid? serviceTypeId)
    {
        List<ZoneRate> items;
        if (zoneId.HasValue)
            items = await _rateService.GetByZoneAsync(zoneId.Value);
        else if (serviceTypeId.HasValue)
            items = await _rateService.GetByServiceTypeAsync(serviceTypeId.Value);
        else
            items = await _rateService.GetAllAsync();

        return Ok(items.Select(MapToRateDto));
    }

    [HttpGet("zone-rates/{id}")]
    public async Task<ActionResult<ZoneRateDto>> GetZoneRate(Guid id)
    {
        var item = await _rateService.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(MapToRateDto(item));
    }

    [HttpPost("zone-rates")]
    public async Task<ActionResult<ZoneRateDto>> CreateZoneRate([FromBody] ZoneRateDto dto)
    {
        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required.");

        var existingRates = await _rateService.GetByZoneAsync(dto.CourierZoneId);
        var dtoEffectiveTo = dto.EffectiveTo ?? DateTime.MaxValue;
        var duplicate = existingRates.FirstOrDefault(r => 
            r.CourierServiceTypeId == dto.CourierServiceTypeId &&
            r.MinWeight <= dto.MaxWeight && dto.MinWeight <= r.MaxWeight &&
            r.EffectiveFrom <= dtoEffectiveTo && dto.EffectiveFrom <= (r.EffectiveTo ?? DateTime.MaxValue));
        
        if (duplicate != null)
            return BadRequest("A rate with the same Zone, Service Type, overlapping weight range, and overlapping dates already exists.");

        var entity = new ZoneRate
        {
            TenantId = tenantId.Value,
            RateName = dto.RateName,
            CourierZoneId = dto.CourierZoneId,
            CourierServiceTypeId = dto.CourierServiceTypeId,
            MinWeight = dto.MinWeight,
            MaxWeight = dto.MaxWeight,
            RateType = Enum.TryParse<RateType>(dto.RateType, out var rt) ? rt : RateType.PerKg,
            BaseRate = dto.BaseRate,
            AdditionalRatePerKg = dto.AdditionalRatePerKg,
            MinCharge = dto.MinCharge,
            FuelSurchargePercent = dto.FuelSurchargePercent,
            CODChargePercent = dto.CODChargePercent,
            CODMinCharge = dto.CODMinCharge,
            EffectiveFrom = DateTime.SpecifyKind(dto.EffectiveFrom, DateTimeKind.Utc),
            EffectiveTo = dto.EffectiveTo.HasValue ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc) : null,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _rateService.CreateAsync(entity);
        return CreatedAtAction(nameof(GetZoneRate), new { id = created.Id }, MapToRateDto(created));
    }

    [HttpPut("zone-rates/{id}")]
    public async Task<ActionResult<ZoneRateDto>> UpdateZoneRate(Guid id, [FromBody] ZoneRateDto dto)
    {
        var existing = await _rateService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        var existingRates = await _rateService.GetByZoneAsync(dto.CourierZoneId);
        var dtoEffectiveTo = dto.EffectiveTo ?? DateTime.MaxValue;
        var duplicate = existingRates.FirstOrDefault(r => 
            r.Id != id &&
            r.CourierServiceTypeId == dto.CourierServiceTypeId &&
            r.MinWeight <= dto.MaxWeight && dto.MinWeight <= r.MaxWeight &&
            r.EffectiveFrom <= dtoEffectiveTo && dto.EffectiveFrom <= (r.EffectiveTo ?? DateTime.MaxValue));
        
        if (duplicate != null)
            return BadRequest("A rate with the same Zone, Service Type, overlapping weight range, and overlapping dates already exists.");

        existing.RateName = dto.RateName;
        existing.CourierZoneId = dto.CourierZoneId;
        existing.CourierServiceTypeId = dto.CourierServiceTypeId;
        existing.MinWeight = dto.MinWeight;
        existing.MaxWeight = dto.MaxWeight;
        existing.RateType = Enum.TryParse<RateType>(dto.RateType, out var rt) ? rt : RateType.PerKg;
        existing.BaseRate = dto.BaseRate;
        existing.AdditionalRatePerKg = dto.AdditionalRatePerKg;
        existing.MinCharge = dto.MinCharge;
        existing.FuelSurchargePercent = dto.FuelSurchargePercent;
        existing.CODChargePercent = dto.CODChargePercent;
        existing.CODMinCharge = dto.CODMinCharge;
        existing.EffectiveFrom = DateTime.SpecifyKind(dto.EffectiveFrom, DateTimeKind.Utc);
        existing.EffectiveTo = dto.EffectiveTo.HasValue ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc) : null;
        existing.IsActive = dto.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        if (existing.CreatedAt.Kind == DateTimeKind.Unspecified)
            existing.CreatedAt = DateTime.SpecifyKind(existing.CreatedAt, DateTimeKind.Utc);

        var updated = await _rateService.UpdateAsync(existing);
        return Ok(MapToRateDto(updated));
    }

    [HttpDelete("zone-rates/{id}")]
    public async Task<ActionResult> DeleteZoneRate(Guid id)
    {
        var deleted = await _rateService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    private static ZoneRateDto MapToRateDto(ZoneRate entity) => new()
    {
        Id = entity.Id,
        CourierZoneId = entity.CourierZoneId,
        ZoneName = entity.CourierZone?.ZoneName,
        CourierServiceTypeId = entity.CourierServiceTypeId,
        ServiceTypeName = entity.CourierServiceType?.Name,
        RateName = entity.RateName,
        MinWeight = entity.MinWeight,
        MaxWeight = entity.MaxWeight,
        RateType = entity.RateType.ToString(),
        BaseRate = entity.BaseRate,
        AdditionalRatePerKg = entity.AdditionalRatePerKg,
        MinCharge = entity.MinCharge,
        FuelSurchargePercent = entity.FuelSurchargePercent,
        CODChargePercent = entity.CODChargePercent,
        CODMinCharge = entity.CODMinCharge,
        EffectiveFrom = entity.EffectiveFrom,
        EffectiveTo = entity.EffectiveTo,
        IsActive = entity.IsActive
    };
    #endregion

    #region Agents
    [HttpGet("agents")]
    public async Task<ActionResult<List<CourierAgentDto>>> GetAgents([FromQuery] string? agentType, [FromQuery] bool activeOnly = false)
    {
        List<CourierAgent> items;
        if (!string.IsNullOrEmpty(agentType) && Enum.TryParse<AgentType>(agentType, out var at))
            items = await _agentService.GetByTypeAsync(at);
        else if (activeOnly)
            items = await _agentService.GetActiveAsync();
        else
            items = await _agentService.GetAllAsync();

        return Ok(items.Select(MapToAgentDto));
    }

    [HttpGet("agents/{id}")]
    public async Task<ActionResult<CourierAgentDto>> GetAgent(Guid id)
    {
        var item = await _agentService.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(MapToAgentDto(item));
    }

    [HttpPost("agents")]
    public async Task<ActionResult<CourierAgentDto>> CreateAgent([FromBody] CourierAgentDto dto)
    {
        if (await _agentService.ExistsAsync(dto.AgentCode))
            return BadRequest("An agent with this code already exists.");

        var tenantId = _tenantProvider.CurrentTenantId;
        if (!tenantId.HasValue)
            return BadRequest("Tenant context required.");

        var entity = new CourierAgent
        {
            TenantId = tenantId.Value,
            AgentCode = dto.AgentCode,
            Name = dto.Name,
            AgentType = Enum.TryParse<AgentType>(dto.AgentType, out var at) ? at : AgentType.DeliveryAgent,
            VendorId = dto.VendorId,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            CommissionPercent = dto.CommissionPercent,
            FixedCommission = dto.FixedCommission,
            BankAccountNo = dto.BankAccountNo,
            BankName = dto.BankName,
            IsActive = dto.IsActive
        };

        var created = await _agentService.CreateAsync(entity);
        return CreatedAtAction(nameof(GetAgent), new { id = created.Id }, MapToAgentDto(created));
    }

    [HttpPut("agents/{id}")]
    public async Task<ActionResult<CourierAgentDto>> UpdateAgent(Guid id, [FromBody] CourierAgentDto dto)
    {
        var existing = await _agentService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        if (await _agentService.ExistsAsync(dto.AgentCode, id))
            return BadRequest("An agent with this code already exists.");

        existing.Name = dto.Name;
        existing.AgentType = Enum.TryParse<AgentType>(dto.AgentType, out var at) ? at : AgentType.DeliveryAgent;
        existing.VendorId = dto.VendorId;
        existing.Phone = dto.Phone;
        existing.Email = dto.Email;
        existing.Address = dto.Address;
        existing.City = dto.City;
        existing.State = dto.State;
        existing.CommissionPercent = dto.CommissionPercent;
        existing.FixedCommission = dto.FixedCommission;
        existing.BankAccountNo = dto.BankAccountNo;
        existing.BankName = dto.BankName;
        existing.IsActive = dto.IsActive;

        var updated = await _agentService.UpdateAsync(existing);
        return Ok(MapToAgentDto(updated));
    }

    [HttpDelete("agents/{id}")]
    public async Task<ActionResult> DeleteAgent(Guid id)
    {
        var deleted = await _agentService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    private static CourierAgentDto MapToAgentDto(CourierAgent entity) => new()
    {
        Id = entity.Id,
        AgentCode = entity.AgentCode,
        Name = entity.Name,
        AgentType = entity.AgentType.ToString(),
        VendorId = entity.VendorId,
        VendorName = entity.Vendor?.Name,
        Phone = entity.Phone,
        Email = entity.Email,
        Address = entity.Address,
        City = entity.City,
        State = entity.State,
        CommissionPercent = entity.CommissionPercent,
        FixedCommission = entity.FixedCommission,
        BankAccountNo = entity.BankAccountNo,
        BankName = entity.BankName,
        IsActive = entity.IsActive
    };
    #endregion
}

public class CourierServiceTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultTransitDays { get; set; }
    public bool AllowCOD { get; set; }
    public bool AllowInsurance { get; set; }
    public bool IsExpress { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CourierZoneDto
{
    public Guid Id { get; set; }
    public string ZoneCode { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ZoneCategoryId { get; set; }
    public string? ZoneCategoryName { get; set; }
    public string ZoneType { get; set; } = "Local";
    public bool IsActive { get; set; }
    
    public List<ZoneCountryDto> Countries { get; set; } = new();
    public List<ZoneStateDto> States { get; set; } = new();
    
    public string CountriesDisplay => Countries.Any() ? string.Join(", ", Countries.Select(c => c.CountryName)) : "-";
    public string StatesDisplay => States.Any() ? string.Join(", ", States.Select(s => s.StateName)) : "-";
}

public class ZoneCountryDto
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ZoneStateDto
{
    public Guid Id { get; set; }
    public Guid StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ZoneRateDto
{
    public Guid Id { get; set; }
    public Guid CourierZoneId { get; set; }
    public string? ZoneName { get; set; }
    public Guid CourierServiceTypeId { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? RateName { get; set; }
    public decimal MinWeight { get; set; }
    public decimal MaxWeight { get; set; }
    public string RateType { get; set; } = "PerKg";
    public decimal BaseRate { get; set; }
    public decimal AdditionalRatePerKg { get; set; }
    public decimal MinCharge { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public decimal CODChargePercent { get; set; }
    public decimal CODMinCharge { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public class CourierAgentDto
{
    public Guid Id { get; set; }
    public string AgentCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AgentType { get; set; } = "DeliveryAgent";
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public decimal CommissionPercent { get; set; }
    public decimal FixedCommission { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankName { get; set; }
    public bool IsActive { get; set; }
}
