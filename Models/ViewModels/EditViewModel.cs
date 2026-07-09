using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlogVue.Models.ViewModels
{
    public class EditViewModel
    {
        public Post Post { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
        [ValidateNever]
        public IFormFile FeaturedImage { get; set; }

    }
}
