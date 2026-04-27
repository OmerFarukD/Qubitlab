using Qubitlab.Identity.Entities;

namespace ProductManagement.Domain.Entities;

public class User : QubitlabUser
{
    public string City { get; set; }
    
    public string ImageUrl { get; set; }
}