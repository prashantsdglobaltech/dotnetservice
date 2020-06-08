

using System.Xml.XPath;

namespace Campaigns.API.Controllers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
  
    using Microsoft.AspNetCore.Mvc;
    using Infrastructure;
    using System.Threading.Tasks;
    using Model;
    using Microsoft.EntityFrameworkCore;
    using Dto;
    using Microsoft.AspNetCore.Authorization;

    using Campaigns.API.ViewModel;    
    using System.Net;
    using Microsoft.Extensions.Options;

    [Route("api/v1/[controller]")]
    [Authorize]
    [ApiController]
    public class CampaignsController : ControllerBase
    {
        private readonly MarketingContext _context;
        private readonly MarketingSettings _settings;
       
        

        public CampaignsController(MarketingContext context,
           
             IOptionsSnapshot<MarketingSettings> settings
           )
        {
            _context = context;
            
            _settings = settings.Value;
           
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CampaignDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CampaignDTO>>> GetAllCampaignsAsync()
        {
            var campaignList = await _context.Campaigns.ToListAsync();

            if (campaignList is null)
            {
                return Ok();
            }

            return MapCampaignModelListToDtoList(campaignList);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CampaignDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<CampaignDTO>> GetCampaignByIdAsync(int id)
        {
            var campaign = await _context.Campaigns.SingleOrDefaultAsync(c => c.Id == id);

            if (campaign is null)
            {
                return NotFound();
            }

            return MapCampaignModelToDto(campaign);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> CreateCampaignAsync([FromBody] CampaignDTO campaignDto)
        {
            if (campaignDto is null)
            {
                return BadRequest();
            }

            var campaign = MapCampaignDtoToModel(campaignDto);

            await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCampaignByIdAsync), new { id = campaign.Id }, null);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> UpdateCampaignAsync(int id, [FromBody] CampaignDTO campaignDto)
        {
            if (id < 1 || campaignDto is null)
            {
                return BadRequest();
            }

            var campaignToUpdate = await _context.Campaigns.FindAsync(id);
            if (campaignToUpdate is null)
            {
                return NotFound();
            }

            campaignToUpdate.Name = campaignDto.Name;
            campaignToUpdate.Description = campaignDto.Description;
            campaignToUpdate.From = campaignDto.From;
            campaignToUpdate.To = campaignDto.To;
            campaignToUpdate.PictureUri = campaignDto.PictureUri;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCampaignByIdAsync), new { id = campaignToUpdate.Id }, null);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<ActionResult> DeleteCampaignByIdAsync(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            var campaignToDelete = await _context.Campaigns.FindAsync(id);

            if (campaignToDelete is null)
            {
                return NotFound();
            }

            _context.Campaigns.Remove(campaignToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("user")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CampaignDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<CampaignDTO>>> GetCampaignsByUserIdAsync( int pageSize = 10, int pageIndex = 0)
        {
           

           

            var campaignDtoList = new List<CampaignDTO>();
            
            
               
                var userCampaignList = await _context.Rules
                    .OfType<UserLocationRule>()
                    .Include(c => c.Campaign)
                    .Where(c => c.Campaign.From <= DateTime.Now
                                && c.Campaign.To >= DateTime.Now   )                            
                                    .Select(c => c.Campaign)
                                    .ToListAsync();

                if (userCampaignList != null && userCampaignList.Any())
                {
                    var userCampaignDtoList = MapCampaignModelListToDtoList(userCampaignList);
                    campaignDtoList.AddRange(userCampaignDtoList);
                }
            

            var totalItems = campaignDtoList.Count();

            campaignDtoList = campaignDtoList
                .Skip(pageSize * pageIndex)
                .Take(pageSize).ToList();

            return new PaginatedItemsViewModel<CampaignDTO>(pageIndex, pageSize, totalItems, campaignDtoList);
        }

        private List<CampaignDTO> MapCampaignModelListToDtoList(List<Campaign> campaignList)
        {
            var campaignDtoList = new List<CampaignDTO>();

            campaignList.ForEach(campaign => campaignDtoList
                .Add(MapCampaignModelToDto(campaign)));

            return campaignDtoList;
        }

        private CampaignDTO MapCampaignModelToDto(Campaign campaign)
        {
           
            var dto = new CampaignDTO
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                From = campaign.From,
                To = campaign.To,
               
            };

            if (!string.IsNullOrEmpty(_settings.CampaignDetailFunctionUri))
            {
                dto.DetailsUri = $"{_settings.CampaignDetailFunctionUri}&campaignId={campaign.Id}&userId=1";
            }

            return dto;
        }

        private Campaign MapCampaignDtoToModel(CampaignDTO campaignDto)
        {
            return new Campaign
            {
                Id = campaignDto.Id,
                Name = campaignDto.Name,
                Description = campaignDto.Description,
                From = campaignDto.From,
                To = campaignDto.To,
                PictureUri = campaignDto.PictureUri
            };
        }

       
    }
}