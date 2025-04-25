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
        //[Authorize] 
        public async Task<IActionResult> CreateInvitationCode(int userId)
        {
            var code = Guid.NewGuid().ToString(); // random code
                                                  // check exist user
            var userExist = await _dBContext.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userExist == null)
            {
                return NotFound("User does not exist.");
            }
            var inviterExists = await _dBContext.InvitationCodes.FirstOrDefaultAsync(u => u.InviterUserId == userExist.UserId);
            if (inviterExists == null)
            {
                var invitationCode = new InvitationCode
                {
                    Code = code,
                    InviterUserId = userExist.UserId,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(1) // expired after 1 days
                };

                _dBContext.InvitationCodes.Add(invitationCode);
                await _dBContext.SaveChangesAsync();
            }
            else
            {
                inviterExists.Code = code;
                inviterExists.CreatedAt = DateTime.UtcNow;
                _dBContext.InvitationCodes.Update(inviterExists);
                await _dBContext.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Invitation code created successfully",
                code
            });
        }


        [HttpPost("use-invitation")]
        //[Authorize]
        public async Task<IActionResult> UseInvitationCode([FromBody] UseInvitationDto useInvitationDto)
        {
            var invitationUserId = useInvitationDto.userId;

            // check invite code
            var invitationCode = await _dBContext.InvitationCodes
                .FirstOrDefaultAsync(i => i.Code == useInvitationDto.Code && i.Status == "active");

            if (invitationCode == null || invitationCode.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired invitation code.");
            }
            var Invitation = await _dBContext.InvitationCodes
                .FirstOrDefaultAsync(i => i.InviterUserId == invitationCode.InviterUserId && i.Status == "active");
            if (Invitation == null)
            {
                return BadRequest("Invalid Invitation");
            }
            // check user exist
            var user = await _dBContext.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == Invitation.InviterUserId);
            if (user == null)
            {
                return NotFound("User does not exist.");
            }
            // check user exist
            var userInvitation = await _dBContext.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == invitationUserId);
            if (userInvitation == null)
            {
                return NotFound("User does not exist.");
            }
            // check relationship exist
            var relationshipExists = await _dBContext.Relationships
                .FirstOrDefaultAsync(r => r.UserId == user.UserId && r.InviterId == userInvitation.UserId);
            if (relationshipExists != null)
            {
                return BadRequest("Relationship already exists.");
            }
            // check relationship equal userId

            if (user.UserId == userInvitation.UserId)
            {
                return BadRequest("Relationship don't create with my mine");
            }
            // create relationship
            var relationship = new Relationship
            {
                UserId = user.UserId,
                InviterId = userInvitation.UserId,
                RelationshipType = "family", // type of relationship
                CreatedAt = DateTime.UtcNow
            };

            // Mark the token has been used
            invitationCode.Status = "used";
            _dBContext.InvitationCodes.Remove(invitationCode);

            _dBContext.Relationships.Add(relationship);
            await _dBContext.SaveChangesAsync();

            return Ok("Invitation code used successfully. Relationship created.");
        }

        [HttpDelete("delete-relationship/{id}")]
        //[Authorize]
        public async Task<IActionResult> DeleteRelationship(int id)
        {
            var relationship = await _dBContext.Relationships
                .FirstOrDefaultAsync(r => r.RelationshipId == id);
            if (relationship == null)
            {
                return NotFound("Relationship not found.");
            }
            _dBContext.Relationships.Remove(relationship);
            await _dBContext.SaveChangesAsync();
            return Ok("Relationship deleted successfully.");
        }
		[HttpGet("get-relationship")]
		//[Authorize]
		public async Task<IActionResult> GetRelationship(int userId)
		{
			
			var relationships = await _dBContext.Relationships
				.Include(r => r.User)
				.Include(r => r.Inviter)
				.Where(r => r.UserId == userId)
				.ToListAsync();
            List< RelationshipDTO> relationshipDTOs = new List< RelationshipDTO>();
			foreach (var relationship in relationships)
            {
                var user = await _dBContext.StrokeUsers.FirstOrDefaultAsync(r => r.UserId == relationship.InviterId);
                var relationshipDTO = new RelationshipDTO
                {
                    RelationshipId = relationship.RelationshipId,
                    UserId = relationship.UserId,
                    InviterId = relationship.InviterId,
                    NameInviter = user.PatientName,
                    EmailInviter = user.Email,
                    RelationshipType = relationship.RelationshipType,
                    CreatedAt = relationship.CreatedAt
                };
                relationshipDTOs.Add(relationshipDTO);

			}
                
			return Ok(relationshipDTOs);
		}


	}


}


