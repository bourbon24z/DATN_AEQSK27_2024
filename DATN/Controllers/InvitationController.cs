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
    public class InvitationController : ControllerBase
    {
        private readonly StrokeDbContext _dbContext;
        public InvitationController(StrokeDbContext context)
        {
            _dbContext = context;
        }

        [HttpPost("create-invitation")]
        [Authorize]
        //http://localhost:5062/api/invitation/create-invitation?userId=123
        public async Task<IActionResult> CreateInvitationCode(int userId)
        {
            try
            {
               
                var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(currentUserIdString, out int currentUserId) || currentUserId != userId)
                {
                    return BadRequest("You can only create invitation codes for yourself.");
                }

               
                var userExists = await _dbContext.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (userExists == null)
                {
                    return NotFound("User does not exist.");
                }

               
                string code = GenerateRandomCode(6);

               
                var inviterExists = await _dbContext.InvitationCodes
                    .FirstOrDefaultAsync(u => u.InviterUserId == userExists.UserId && u.Status == "active");

                if (inviterExists == null)
                {
                    
                    var invitationCode = new InvitationCode
                    {
                        Code = code,
                        InviterUserId = userExists.UserId,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(7) // Hết hạn sau 7 ngày
                    };

                    _dbContext.InvitationCodes.Add(invitationCode);
                }
                else
                {
                    
                    inviterExists.Code = code;
                    inviterExists.CreatedAt = DateTime.UtcNow;
                    inviterExists.ExpiresAt = DateTime.UtcNow.AddDays(7);
                    _dbContext.InvitationCodes.Update(inviterExists);
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Invitation code created successfully",
                    code,
                    expiresAt = DateTime.UtcNow.AddDays(7)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("use-invitation")]
        [Authorize]
        //http://localhost:5062/api/invitation/use-invitation
        public async Task<IActionResult> UseInvitationCode([FromBody] UseInvitationDto useInvitationDto)
        {
            try
            {
                
                var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(currentUserIdString, out int currentUserId))
                {
                    return BadRequest("Invalid user identifier.");
                }

               
                if (currentUserId != useInvitationDto.UserId)
                {
                    return BadRequest("You can only use invitation codes for yourself.");
                }

                
                var invitationCode = await _dbContext.InvitationCodes
                    .Include(i => i.InviterUser)
                    .FirstOrDefaultAsync(i => i.Code == useInvitationDto.Code && i.Status == "active");

                if (invitationCode == null || invitationCode.ExpiresAt < DateTime.UtcNow)
                {
                    return BadRequest("Invalid or expired invitation code.");
                }

                var inviter = invitationCode.InviterUser;
                if (inviter == null)
                {
                    return NotFound("Inviter does not exist.");
                }

               
                var user = await _dbContext.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == useInvitationDto.UserId);
                if (user == null)
                {
                    return NotFound("User does not exist.");
                }

                
                var relationshipExists = await _dbContext.Relationships
                    .AnyAsync(r =>
                        (r.UserId == user.UserId && r.InviterId == inviter.UserId) ||
                        (r.UserId == inviter.UserId && r.InviterId == user.UserId));

                if (relationshipExists)
                {
                    return BadRequest("Relationship already exists.");
                }

               
                if (inviter.UserId == user.UserId)
                {
                    return BadRequest("You cannot create a relationship with yourself.");
                }

                
                string relationshipType = "family"; // default

                // check doctor
                bool isInviterDoctor = await _dbContext.UserRoles
                    .AnyAsync(ur => ur.UserId == inviter.UserId && ur.Role.RoleName == "doctor" && ur.IsActive);

                if (isInviterDoctor)
                {
                    relationshipType = "doctor-patient";
                }

               
                var relationship = new Relationship
                {
                    UserId = user.UserId,
                    InviterId = inviter.UserId,
                    RelationshipType = relationshipType,
                    CreatedAt = DateTime.UtcNow
                };

                
                invitationCode.Status = "used";

                _dbContext.Relationships.Add(relationship);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Invitation code used successfully. Relationship created.",
                    relationshipType,
                    inviterName = inviter.PatientName,
                    inviterId = inviter.UserId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("delete-relationship/{id}")]
        [Authorize]
        //http://localhost:5062/api/invitation/delete-relationship/123
        public async Task<IActionResult> DeleteRelationship(int id)
        {
            try
            {
                
                var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(currentUserIdString, out int currentUserId))
                {
                    return BadRequest("Invalid user identifier.");
                }

                var relationship = await _dbContext.Relationships
                    .FirstOrDefaultAsync(r => r.RelationshipId == id);

                if (relationship == null)
                {
                    return NotFound("Relationship not found.");
                }

                
                if (relationship.UserId != currentUserId && relationship.InviterId != currentUserId)
                {
                    return Forbid("You don't have permission to delete this relationship.");
                }

                _dbContext.Relationships.Remove(relationship);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Relationship deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("get-relationship")]
        [Authorize]
        //http://localhost:5062/api/invitation/get-relationship?userId=123&type=family
        public async Task<IActionResult> GetRelationship(int userId, [FromQuery] string type = null)
        {
            try
            {
                
                var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(currentUserIdString, out int currentUserId))
                {
                    return BadRequest("Invalid user identifier.");
                }

               
                bool isAdmin = User.IsInRole("admin");

               
                if (!isAdmin && currentUserId != userId)
                {
                    return Forbid("You don't have permission to view these relationships.");
                }

                
                var relationshipsQuery = _dbContext.Relationships
                    .Include(r => r.User)
                    .Include(r => r.Inviter)
                    .Where(r => r.UserId == userId || r.InviterId == userId);

                
                if (!string.IsNullOrEmpty(type))
                {
                    relationshipsQuery = relationshipsQuery.Where(r => r.RelationshipType == type);
                }

                
                var relationships = await relationshipsQuery.ToListAsync();

                List<RelationshipDTO> relationshipDTOs = new List<RelationshipDTO>();

                foreach (var relationship in relationships)
                {
                    
                    var partnerId = relationship.UserId == userId ? relationship.InviterId : relationship.UserId;
                    var partner = await _dbContext.StrokeUsers.FirstOrDefaultAsync(r => r.UserId == partnerId);

                    if (partner != null)
                    {
                        var relationshipDTO = new RelationshipDTO
                        {
                            RelationshipId = relationship.RelationshipId,
                            UserId = relationship.UserId,
                            InviterId = relationship.InviterId,
                            NameInviter = partner.PatientName,
                            EmailInviter = partner.Email,
                            RelationshipType = relationship.RelationshipType,
                            CreatedAt = relationship.CreatedAt
                        };

                        relationshipDTOs.Add(relationshipDTO);
                    }
                }

                return Ok(relationshipDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}