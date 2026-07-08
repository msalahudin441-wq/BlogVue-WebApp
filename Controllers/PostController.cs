using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BlogVue.Data;
using BlogVue.Models;
using BlogVue.Models.ViewModels;
using System.Net;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost]
        public async Task<IActionResult>Create(PostViewModel postViewModel)
        {
            if (ModelState.IsValid)
            {
                var inputFileExtension= Path.GetExtension(postViewModel.FeaturedImage.FileName).ToLower();
                bool isAllowed= _allowedExtensions.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image Format. Allowed Formats are jpg,jpeg and png");
                    return View(postViewModel);
                }
              postViewModel.Post.FeaturedImagePath =await UploadFileToFolder(postViewModel.FeaturedImage);
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