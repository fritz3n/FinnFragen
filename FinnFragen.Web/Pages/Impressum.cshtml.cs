using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace FinnFragen.Web.Pages;

public class ImpressumModel(IConfiguration configuration) : PageModel
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    public void OnGet()
    {
        IConfigurationSection impressum = configuration.GetSection("Impressum");
        this.Name = impressum[nameof(this.Name)];
        this.Address = impressum[nameof(this.Address)];
        this.City = impressum[nameof(this.City)];
        this.Phone = impressum[nameof(this.Phone)];
        this.Email = impressum[nameof(this.Email)];
    }
}
