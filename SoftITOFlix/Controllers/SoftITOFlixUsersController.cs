﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftITOFlix.Data;
using SoftITOFlix.Models;

namespace SoftITOFlix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoftITOFlixUsersController : ControllerBase
    {
        public struct LogInModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        private readonly SignInManager<SoftITOFlixUser> _signInManager;
        private readonly SoftITOFlixContext _context;

        public SoftITOFlixUsersController(SignInManager<SoftITOFlixUser> signInManager, SoftITOFlixContext context)
        {
            _signInManager = signInManager;
            _context = context;
        }

        // GET: api/SoftITOFlixUsers
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult<List<SoftITOFlixUser>> GetUsers(bool includePassive = true)
        {
            IQueryable<SoftITOFlixUser> users = _signInManager.UserManager.Users;
            
            if (includePassive == false)
            {
                users = users.Where(u => u.Passive == false);
            }
            return users.AsNoTracking().ToList();
        }

        // GET: api/SoftITOFlixUsers/5
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<SoftITOFlixUser> GetSoftITOFlixUser(long id)
        {
            SoftITOFlixUser? softITOFlixUser = null;

            if (User.IsInRole("Administrator") == false)
            {
                if (User.FindFirstValue(ClaimTypes.NameIdentifier) != id.ToString())
                {
                    return Unauthorized();
                }
            }
            softITOFlixUser = _signInManager.UserManager.Users.Where(u => u.Id == id).AsNoTracking().FirstOrDefault();

            if (softITOFlixUser == null)
            {
                return NotFound();
            }

            return softITOFlixUser;
        }

        // PUT: api/SoftITOFlixUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize]
        public ActionResult PutSoftITOFlixUser(SoftITOFlixUser softITOFlixUser)
        {
            SoftITOFlixUser? user = null;

            if (User.IsInRole("CustomerRepresentative") == false)
            {
                if (User.FindFirstValue(ClaimTypes.NameIdentifier) != softITOFlixUser.Id.ToString())
                {
                    return Unauthorized();
                }
            }
            user = _signInManager.UserManager.Users.Where(u => u.Id == softITOFlixUser.Id).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }
            user.PhoneNumber = softITOFlixUser.PhoneNumber;
            user.BirthDate = softITOFlixUser.BirthDate;
            user.Email = softITOFlixUser.Email;
            user.Name = softITOFlixUser.Name;
            _signInManager.UserManager.UpdateAsync(user).Wait();
            return Ok();
        }

        // POST: api/SoftITOFlixUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<string> PostSoftITOFlixUser(SoftITOFlixUser softITOFlixUser)
        {
            if(User.Identity!.IsAuthenticated==true)
            {
                return BadRequest();
            }
            IdentityResult identityResult = _signInManager.UserManager.CreateAsync(softITOFlixUser, softITOFlixUser.Password).Result;

            if (identityResult != IdentityResult.Success)
            {
                return identityResult.Errors.FirstOrDefault()!.Description;
            }
            return Ok();
        }

        // DELETE: api/SoftITOFlixUsers/5
        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult DeleteSoftITOFlixUser(long id)
        {
            SoftITOFlixUser? user = null;

            if (User.IsInRole("Administrator") == false)
            {
                if (User.FindFirstValue(ClaimTypes.NameIdentifier) != id.ToString())
                {
                    return Unauthorized();
                }
            }
            user = _signInManager.UserManager.Users.Where(u => u.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }
            user.Passive = true;
            _signInManager.UserManager.UpdateAsync(user).Wait();
            return Ok();
        }

        [HttpPost("LogIn")]
        public ActionResult<List<Media>?> LogIn(LogInModel logInModel)
        {
            Microsoft.AspNetCore.Identity.SignInResult signInResult;
            SoftITOFlixUser applicationUser = _signInManager.UserManager.FindByNameAsync(logInModel.UserName).Result;
            List<Media>? medias = null;
            IQueryable<UserFavorite> userFavorites;
            IGrouping<short, MediaCategory>? mediaCategories;
            IQueryable<Media> mediaQuery;
            IQueryable<int> userWatcheds;

            if (applicationUser == null)
            {
                return NotFound();
            }
            if (_context.UserPlans.Where(u => u.UserId == applicationUser.Id && u.EndDate >= DateTime.Today).Any() == false)
            {
                applicationUser.Passive = true;
                _signInManager.UserManager.UpdateAsync(applicationUser).Wait();
            }
            if(applicationUser.Passive == true)
            {
                return Content("Passive");
            }
            signInResult = _signInManager.PasswordSignInAsync(applicationUser, logInModel.Password, false, false).Result;
            if (signInResult.Succeeded == true)
            {
                userFavorites = _context.UserFavorites.Where(u => u.UserId == applicationUser.Id);
                userFavorites = userFavorites.Include(u => u.Media);
                userFavorites = userFavorites.Include(u => u.Media!.MediaCategories);
                List<UserFavorite> favorites = userFavorites.ToList();
                mediaCategories = userFavorites.ToList().SelectMany(u => u.Media!.MediaCategories!).GroupBy(m => m.CategoryId).OrderByDescending(m => m.Count()).FirstOrDefault();
                if (mediaCategories != null)
                {
                    userWatcheds = _context.UserWatcheds.Where(u => u.UserId == applicationUser.Id).Include(u => u.Episode).Select(u => u.Episode!.MediaId).Distinct();
                    mediaQuery = _context.Medias.Include(m => m.MediaCategories!.Where(mc => mc.CategoryId == mediaCategories.Key)).Where(m=>m.MediaCategories!=null).Where(m => userWatcheds.Contains(m.Id) == false);
                    if(applicationUser.Restriction!=null)
                    {
                        mediaQuery = mediaQuery.Include(m => m.MediaRestrictions!.Where(r => r.RestrictionId <= applicationUser.Restriction));
                    }
                    medias = mediaQuery.ToList();
                }
                //Populate medias
            }
            return medias;
        }
    }
}
