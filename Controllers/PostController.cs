using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BlogVue.Data;
using BlogVue.Models;
using BlogVue.Models.ViewModels;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BlogVue.Controllers
{
    public class PostController : Controller
    {
        private readonly ApDbContext _context;
        private readonly string[] _allowedExtensions = {".jpg",".jpeg",".png"};
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(ApDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public IActionResult Index(int ? categoryId)
        {
            var postQuery = _context.Posts.Include(p => p.Category).AsQueryable();
            if (categoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryId);
            }
            var posts = postQuery.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View(posts);
        }
        [HttpGet]
       
        public async Task<IActionResult> Detail(int id)
        {
            if(id== null)
            {
                return NotFound();
            }
            var post = _context.Posts.Include(p => p.Category).Include(p => p.Comments).FirstOrDefault
                (p => p.Id == id);
            if(post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            var postViewModel = new PostViewModel
            {
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(postViewModel);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            var postFromDb = await _context.Posts.FirstOrDefaultAsync(p=>p.Id == id);
            if(postFromDb == null)
            {
                return NotFound();
            }
            EditViewModel editViewModel = new EditViewModel
            {
                Post = postFromDb,
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };
            return View(editViewModel);

        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            if (ModelState.IsValid)
            {
                var inputFileExtension = Path.GetExtension(postViewModel.FeaturedImage.FileName).ToLower();
                bool isAllowed = _allowedExtensions.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image Format. Allowed Formats are jpg,jpeg and png");
                    return View(postViewModel);
                }
                postViewModel.Post.FeaturedImagePath = await UploadFileToFolder(postViewModel.FeaturedImage);
                await _context.Posts.AddAsync(postViewModel.Post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            postViewModel.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return View(postViewModel);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditViewModel editViewModel)
        {
            if(!ModelState.IsValid)
            {
               return View(editViewModel);
            }
            var postFromDb =  await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == editViewModel.Post.Id);
            if (postFromDb == null)
            {
                return NotFound();
            }
            if (editViewModel.FeaturedImage != null)
            {
                var inputFileExtension = Path.GetExtension(editViewModel.FeaturedImage.FileName).ToLower();
                bool isAllowed = _allowedExtensions.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image Format. Allowed Formats are jpg,jpeg and png");
                    return View(editViewModel);
                }
                var ExistingImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", Path.GetFileName(postFromDb.FeaturedImagePath));
                if (System.IO.File.Exists(ExistingImagePath))
                {
                    System.IO.File.Delete(ExistingImagePath);
                }
                editViewModel.Post.FeaturedImagePath = await UploadFileToFolder(editViewModel.FeaturedImage);

            }
            else
            {
                editViewModel.Post.FeaturedImagePath=postFromDb.FeaturedImagePath;
            }
            _context.Posts.Update(editViewModel.Post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
                  
                
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var postFromDb= await _context.Posts.FirstOrDefaultAsync(p=> p.Id == id);
            if(postFromDb == null)
            {
                return NotFound();
            }
            return View(postFromDb);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (string.IsNullOrEmpty(postFromDb.FeaturedImagePath))
            {
                var ExistingImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", Path.GetFileName(postFromDb.FeaturedImagePath));
                if (System.IO.File.Exists(ExistingImagePath))
                {
                    System.IO.File.Delete(ExistingImagePath);
                }

            }
            _context.Posts.Remove(postFromDb);
            _context.SaveChangesAsync();

            return RedirectToAction("Index");

        }

        [HttpPost]
        [Authorize]
        public JsonResult AddComment([FromBody]Comments comment)
        {
            comment.CommentDate = DateTime.Now;
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return Json(
                new
                {
                    username = comment.UserName,
                    commentDate = comment.CommentDate.ToString("MMM, dd, yyyy"),
                    content = comment.Content
                }
                );
        }
        [HttpPost]
       

        private async Task<string> UploadFileToFolder(IFormFile file)
        {
            var inputFileExtension = Path.GetExtension(file.FileName);
            var fileName= Guid.NewGuid().ToString() + inputFileExtension;
            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var imagesFolderPath = Path.Combine(wwwRootPath, "images");
            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }
            var filePath= Path.Combine(imagesFolderPath, fileName);
            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                return "Error uploading image" + ex.Message;
            }
            return "/images/" + fileName;
        }
       
    }
}