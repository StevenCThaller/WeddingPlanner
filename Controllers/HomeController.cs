using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public ViewResult LogReg()
        {
            return View("LogReg");
        }

        [HttpPost("users/register")]
        public IActionResult Register(LogRegWrapper FromForm)
        {
            if(ModelState.IsValid)
            {
                // Unique validation
                if(dbContext.Users.Any(u => u.Email == FromForm.Register.Email))
                {
                    ModelState.AddModelError("Register.Email", "Already registered? Please Log In.");
                    return LogReg();
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                FromForm.Register.Password = Hasher.HashPassword(FromForm.Register, FromForm.Register.Password);
                
                dbContext.Add(FromForm.Register);
                dbContext.SaveChanges();


                HttpContext.Session.SetInt32("UserId",  FromForm.Register.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return LogReg();
            }
        }

        [HttpPost("users/login")]
        public IActionResult Login(LogRegWrapper FromForm)
        {
            if(ModelState.IsValid)
            {
                User InDb = dbContext.Users.FirstOrDefault(u => u.Email == FromForm.Login.Email);

                if(InDb == null)
                {
                    ModelState.AddModelError("Login.Email", "Invalid email/password");
                    return LogReg();
                }

                PasswordHasher<LogUser> Hasher = new PasswordHasher<LogUser>();
                PasswordVerificationResult Result = Hasher.VerifyHashedPassword(FromForm.Login, InDb.Password, FromForm.Login.Password);

                if(Result == 0)
                {
                    ModelState.AddModelError("Login.Email", "Invalid email/password");
                    return LogReg();
                }
                HttpContext.Session.SetInt32("UserId", InDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return LogReg();
            }
        }

        [HttpGet("logout")]
        public RedirectToActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("LogReg");
        }


        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }

            DashboardWrapper WMod = new DashboardWrapper()
            {
                AllWeddings = dbContext.Weddings
                    .Include(w => w.Planner)
                    .Include(w => w.GuestsAttending)
                    .ThenInclude(r => r.Guest)
                    .Where(w => w.Date > DateTime.Today)
                    .ToList(),
                LoggedUser = dbContext.Users
                    .FirstOrDefault(u => u.UserId == (int)LoggedId)
            };

            return View("Dashboard", WMod);
        }

        [HttpGet("weddings/new")]
        public IActionResult NewWedding()
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }

            return View("NewWedding");
        }

        [HttpPost("weddings/create")]
        public IActionResult CreateWedding(Wedding FromForm)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }
            // Attach the user who planned the wedding to the object.
            FromForm.UserId = (int)LoggedId;

            if(ModelState.IsValid)
            {
                if(FromForm.Date < DateTime.Today)
                {
                    ModelState.AddModelError("Date", "No time travel!");
                    return NewWedding();
                }


                dbContext.Add(FromForm);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                return NewWedding();
            }
        }

        [HttpGet("weddings/{WeddingId}")]
        public IActionResult WeddingDetail(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }

            Wedding ToPage = dbContext.Weddings
                .Include(w => w.GuestsAttending)
                .ThenInclude(r => r.Guest)
                .FirstOrDefault(w => w.WeddingId == WeddingId);

            if(ToPage == null)
            {
                return RedirectToAction("Dashboard");
            }

            return View("WeddingDetail", ToPage);
        }

        [HttpGet("weddings/{WeddingId}/edit")]
        public IActionResult EditWedding(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }

            Wedding ToEdit = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == WeddingId);

            if(ToEdit == null || ToEdit.UserId != (int)LoggedId)
            {
                return RedirectToAction("Dashboard");
            }

            return View("EditWedding", ToEdit);
        }

        [HttpPost("weddings/{WeddingId}/update")]
        public IActionResult UpdateWedding(int WeddingId, Wedding FromForm)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }

            if(!dbContext.Weddings.Any(w => w.WeddingId == WeddingId && w.UserId == (int)LoggedId))
            {
                return RedirectToAction("Dashboard");
            }
            FromForm.UserId = (int)LoggedId;
            if(ModelState.IsValid)
            {
                FromForm.WeddingId = WeddingId;
                dbContext.Update(FromForm);
                dbContext.Entry(FromForm).Property("CreatedAt").IsModified = false;
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                return EditWedding(WeddingId);
            }
        }


        [HttpGet("weddings/{WeddingId}/rsvp")]
        public RedirectToActionResult RSVP(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }
            Wedding ToJoin = dbContext.Weddings
                .Include(w => w.GuestsAttending)
                .FirstOrDefault(w => w.WeddingId == WeddingId);

            if(ToJoin == null || ToJoin.UserId == (int)LoggedId || ToJoin.GuestsAttending.Any(r => r.UserId == (int)LoggedId))
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                RSVP NewRsvp = new RSVP()
                {
                    UserId = (int)LoggedId,
                    WeddingId = WeddingId
                };
                dbContext.Add(NewRsvp);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet("weddings/{WeddingId}/unrsvp")]
        public RedirectToActionResult UnRSVP(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }
            Wedding ToLeave = dbContext.Weddings
                .Include(w => w.GuestsAttending)
                .FirstOrDefault(w => w.WeddingId == WeddingId);

            if(ToLeave == null || !ToLeave.GuestsAttending.Any(r => r.UserId == (int)LoggedId))
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                RSVP ToRemove = dbContext.RSVPs.FirstOrDefault(r => r.UserId == (int)LoggedId && r.WeddingId == WeddingId);
                dbContext.Remove(ToRemove);
                dbContext.SaveChanges();

                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet("weddings/{WeddingId}/delete")]
        public RedirectToActionResult DeleteWedding(int WeddingId)
        {
            int? LoggedId = HttpContext.Session.GetInt32("UserId");
            if(LoggedId == null)
            {
                return RedirectToAction("LogReg");
            }

            Wedding ToDelete = dbContext.Weddings
                .FirstOrDefault(w => w.WeddingId == WeddingId);

            if(ToDelete == null || ToDelete.UserId != (int)LoggedId)
            {
                return RedirectToAction("Dashboard");
            }

            dbContext.Remove(ToDelete);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }
    }

}