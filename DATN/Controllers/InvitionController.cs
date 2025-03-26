using DATN.Models;
using DATN.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using DATN.Dto;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvitionController : ControllerBase
    {
        private readonly StrokeDbContext _dBContext;
        public InvitionController(StrokeDbContext context)
        {
            _dBContext = context;
        }

        [HttpPost("create-invitation")]
        [Authorize] 
        public async Task<IActionResult> CreateInvitationCode()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Kiểm tra xem user có tồn tại không
            var inviterExists = await _dBContext.StrokeUsers.AnyAsync(u => u.UserId == currentUserId);
            if (!inviterExists)
            {
                return BadRequest("User does not exist.");
            }

            // create invite code
            var invitationCode = new InvitationCode
            {
                Code = Guid.NewGuid().ToString(), // random token
                InviterUserId = currentUserId,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1) // expired after 1 days
            };

            _dBContext.InvitationCodes.Add(invitationCode);
            await _dBContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Invitation code created successfully",
                code = invitationCode.Code
            });
        }


        [HttpPost("use-invitation")]
        [Authorize]
        public async Task<IActionResult> UseInvitationCode([FromBody] UseInvitationDto useInvitationDto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // check invite code
            var invitationCode = await _dBContext.InvitationCodes
                .FirstOrDefaultAsync(i => i.Code == useInvitationDto.Code && i.Status == "active");

            if (invitationCode == null || invitationCode.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired invitation code.");
            }

            // create relationship
            var relationship = new Relationship
            {
                UserId = currentUserId,
                InviterId = invitationCode.InviterUserId,
                RelationshipType = "family", // type of relationship
                CreatedAt = DateTime.UtcNow
            };

             // Mark the token has been used
            invitationCode.Status = "used";

            _dBContext.Relationships.Add(relationship);
            await _dBContext.SaveChangesAsync();

            return Ok("Invitation code used successfully. Relationship created.");
        }

    }
}
