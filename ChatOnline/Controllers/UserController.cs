using ChatOnline.Database;
using ChatOnline.Interface;
using ChatOnline.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace ChatOnline.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : ControllerBase
    {
        private readonly IJWTService jwt;
        private readonly ApplicationDbContext context;
        private readonly IHostingEnvironment environment;

        public UserController(IJWTService jwt, ApplicationDbContext context, IHostingEnvironment environment)
        {
            this.jwt = jwt;
            this.context = context;
            this.environment = environment;
        }

        [HttpGet("Room")]
        public List<Room> GetAllRooms() => context.Rooms.ToList();

        [HttpPost("Register")]
        public User Register([FromBody] User user)
        {
            user.Name = user.Login;
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] User user)
        {
            var returnValue = context.Users.SingleOrDefault(x => x.Login.Equals(user.Login));
            if (returnValue == null)
            {
                return NotFound(new {message = "User not found in db"});
            }

            return Ok(new {token = jwt.Generate(returnValue)});
        }

        [HttpGet("Logout")]
        public IActionResult Logout(int id) => Ok(new
        {
            success = "success"
        });

        [HttpPost("Upload"), DisableRequestSizeLimit]
        public IActionResult Upload(int userId)
        {
            try
            {
                var file = Request.Form.Files[0];
                string folderName = "Uploads";
                string webRootPath = environment.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                if (file.Length > 0)
                {
                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string fullPath = Path.Combine(newPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    var user = context.Users.Single(x => x.Id == userId);
                    user.FileName = fileName;
                    context.Update(user);
                    context.SaveChanges();
                }

                return Ok(new {response = "Good job!"});
            }
            catch (System.Exception ex)
            {
                return BadRequest("Upload Failed: " + ex.Message);
            }
        }

        [HttpGet("Download")]
        public IActionResult Download(string photoName)
        {
            var content = $"http://{Request.Host}/uploads/{photoName}";
            return Ok(new {url = content});
        }
    }
}