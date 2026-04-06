namespace ZooShop.Models;

public class Pet
{
    public int PetID { get; set; }
    public int ClientID { get; set; }
    public string PetName { get; set; } = string.Empty;
    public int AnimalTypeID { get; set; }
    public string? Breed { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public decimal? Weight { get; set; }
    public string? Allergies { get; set; }
    public string? SpecialNeeds { get; set; }
    public string? PhotoUrl { get; set; }

    public Client Client { get; set; } = null!;
    public AnimalType AnimalType { get; set; } = null!;
}
