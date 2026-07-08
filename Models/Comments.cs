using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlogVue.Models
{
    public class Comments
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "User Name is required")]
        [MaxLength(100, ErrorMessage = "User Name cannot exceed 100 characters")] 
        public string UserName {  get; set; }
        [DataType(DataType.Date)]
        public DateTime CommentDate { get; set; }
        [Required(ErrorMessage = "Content is required") ]
        public string Content { get; set; }
        [ForeignKey("Post")]
        public int PostId { get; set; }
         public Post Post { get; set; }

    }


}
