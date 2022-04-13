using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels.Accounts;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace Blog.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly TokenService _tokenService;

        public AccountController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }


        [AllowAnonymous]
        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = await context
                .Users
                .AsNoTracking()
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            // um dos jeitos de inserir uma Role
            //var role = await context.Roles.FirstOrDefaultAsync(x => x.Id == 1);
            //user.Roles.Add(role);
            //context.Users.Update(user);
            //await context.SaveChangesAsync();



            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválida"));

            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválida"));

            try
            {
                var token = _tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));

            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }


        }

        [HttpPost("v1/accounts/")]
        public async Task<IActionResult> Post([FromBody] RegisterViewModel model,
            [FromServices] BlogDataContext context,
            [FromServices] EmailService emailService)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));


            var user = await context
                .Users
                .AsNoTracking()
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            var userRole = new UserRole
            {
                User = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Slug = model.Email.Replace("@", "-").Replace(".", "-"),
                },

                RoleId = model.IdRole            
            };


            // Rota do Update 
            //var userRole = user.UserRoles.First();
            //userRole.RoleId = 2;

            //await context.UserRoles.Update(userRole)




            var password = PasswordGenerator.Generate(25);
            userRole.User.PasswordHash = PasswordHasher.Hash(password);

            try
            {
                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();

                // procurar smtp válido n funciona pois n tenho
                //emailService.Send(
                //    user.Name,
                //    user.Email,
                //    subject: "Bem vindo",
                //    body: $"sua senha é <strong>{password}</strong>");

                return Ok(new ResultViewModel<dynamic>(new
                {
                    password
                }));
            }
            catch (DbUpdateException)

            {
                return StatusCode(400, new ResultViewModel<string>("05X99 - Este E-mail ja está cadastrado"));
            }

            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }


        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage([FromBody] UploadImageViewModel model,
            [FromServices] BlogDataContext context)
        {
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);

            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05x04 - Falha Interna no servidor"));
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if (user == null)
                return NotFound(new ResultViewModel<string>("Usuário não encontrado"));

            user.Image = $"https://localhost:0000/images/{fileName}";

            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05x04 - Falha Interna no servidor"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso", null));


        }

        //[Authorize(Roles = "user" )]
        //[HttpGet("v1/user")]
        //public IActionResult GetUser() => Ok(User.Identity.Name);

        //[Authorize(Roles = "author")]
        //[HttpGet("v1/author")]
        //public IActionResult GetAuthor() => Ok(User.Identity.Name);

        //[Authorize(Roles = "admin")]
        //[HttpGet("v1/admin")]
        //public IActionResult GetAdmin() => Ok(User.Identity.Name);
    }
}
