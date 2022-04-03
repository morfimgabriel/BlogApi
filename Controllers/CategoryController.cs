using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    [Route("v1")]
    public class CategoryController : ControllerBase
    {
        [HttpGet("categories")]
        public async Task<IActionResult> GetAsync([FromServices]BlogDataContext context)
        {
            try
            {
                var categories = await context.Categories.ToListAsync();
                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05X13 - Falha interna no Servidor"));
            }
           
        }

        [HttpGet("categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id, [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (category == null)
                    return NotFound(new ResultViewModel<Category>("05X13 - Conteúdo não encontrado"));

                return Ok(new ResultViewModel<Category>(category));
            }

            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05X13 - Falha interna no Servidor"));
            }
           
        }

        [HttpPost("categories/")]
        public async Task<IActionResult> PostAsync([FromBody] EditorCategoryViewModel model, [FromServices] BlogDataContext context)
        {

            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

            try
            {
                var category = new Category
                {
                    Id = 0,
                    Name = model.Name,
                    Slug = model.Slug.ToLower(),
                };

                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();

                return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));

            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05XE9 - Não foi possível incluir a categoria"));
            }

            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05X10 - Falha interna no Servidor"));
            }


        }

        [HttpPut("categories/{id:int}")]
        public async Task<IActionResult> PutAsync([FromRoute] int id, [FromBody] EditorCategoryViewModel model, [FromServices] BlogDataContext context)
        {

            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

                category.Name = model.Name;
                category.Slug = model.Slug;

                context.Categories.Update(category);
                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(category));
            }

            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE12 - Não foi possível alterar a categoria"));
            }

            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE13 - Falha interna no Servidor"));
            }
           
        }

        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id, [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

                context.Categories.Remove(category);
                await context.SaveChangesAsync();


                return Ok(new ResultViewModel<Category>(category));
            }
             catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE10 - Não foi possível deletar a categoria"));
            }

            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("05X11 - Falha interna no Servidor"));
            }
        }


    }
}
