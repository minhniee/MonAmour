using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MonAmour.ViewModels;

public class AboutUsModel : PageModel
{
    [BindProperty]
    public string Name { get; set; }

    [BindProperty]
    public string Email { get; set; }

    [BindProperty]
    public string Phone { get; set; }

    [BindProperty]
    public string Address { get; set; }

    [BindProperty]
    public string Message { get; set; }

    public void OnGet()
    {
    }

}