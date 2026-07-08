using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BlogVue.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(400,ErrorMessage = "Title cannot exceed 400 characters")]
        public string Title { get; set; }
        [Required(ErrorMessage ="The content is required")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Author is required")]
        [MaxLength(100, ErrorMessage = "Author cannot exceed 100 characters")]
        public string Author { get; set; }
        [ValidateNever]
        public string FeaturedImagePath { get; set; }
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; }= DateTime.Now;
        [ForeignKey("Category")]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public ICollection<Comments> Comments { get; set; }
    }
}
