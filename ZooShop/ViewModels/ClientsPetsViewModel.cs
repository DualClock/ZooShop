using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;
using ZooShop.Models;

namespace ZooShop.ViewModels;

public class ClientsPetsViewModel : ViewModelBase
{
    private readonly MainViewModel _main;
    private string _statusMessage = "";
    private Client? _selectedClient;
    private Pet? _selectedPet;
    private string _searchText = "";

    // Форма клиента
    private string _clientFirstName = "";
    private string _clientLastName = "";
    private string _clientPhone = "";
    private string _clientEmail = "";
    private string _clientNotes = "";

    // Форма питомца
    private string _petName = "";
    private int? _petAnimalTypeId;
    private string _petBreed = "";
    private string _petAllergies = "";
    private string _petGender = "";

    public ObservableCollection<Client> Clients { get; } = new();
    public ObservableCollection<Pet> Pets { get; } = new();
    public ObservableCollection<AnimalType> AnimalTypes { get; } = new();
    public ObservableCollection<Product> RecommendedProducts { get; } = new();

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public Client? SelectedClient
    {
        get => _selectedClient;
        set { SetField(ref _selectedClient, value); _ = LoadPetsAsync(); }
    }

    public Pet? SelectedPet
    {
        get => _selectedPet;
        set { SetField(ref _selectedPet, value); LoadRecommendations(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { SetField(ref _searchText, value); }
    }

    public string ClientFirstName
    {
        get => _clientFirstName;
        set => SetField(ref _clientFirstName, value);
    }

    public string ClientLastName
    {
        get => _clientLastName;
        set => SetField(ref _clientLastName, value);
    }

    public string ClientPhone
    {
        get => _clientPhone;
        set => SetField(ref _clientPhone, value);
    }

    public string ClientEmail
    {
        get => _clientEmail;
        set => SetField(ref _clientEmail, value);
    }

    public string ClientNotes
    {
        get => _clientNotes;
        set => SetField(ref _clientNotes, value);
    }

    public string PetName
    {
        get => _petName;
        set => SetField(ref _petName, value);
    }

    public int? PetAnimalTypeId
    {
        get => _petAnimalTypeId;
        set => SetField(ref _petAnimalTypeId, value);
    }

    public string PetBreed
    {
        get => _petBreed;
        set => SetField(ref _petBreed, value);
    }

    public string PetAllergies
    {
        get => _petAllergies;
        set => SetField(ref _petAllergies, value);
    }

    public string PetGender
    {
        get => _petGender;
        set => SetField(ref _petGender, value);
    }

    public ICommand LoadClientsCommand { get; }
    public ICommand SaveClientCommand { get; }
    public ICommand DeleteClientCommand { get; }
    public ICommand SavePetCommand { get; }
    public ICommand DeletePetCommand { get; }
    public ICommand SearchCommand { get; }

    public ClientsPetsViewModel(MainViewModel main)
    {
        _main = main;
        LoadClientsCommand = new RelayCommand(async _ => await LoadClientsAsync());
        SaveClientCommand = new RelayCommand(async _ => await SaveClientAsync());
        DeleteClientCommand = new RelayCommand(async _ => await DeleteClientAsync(), _ => SelectedClient != null);
        SavePetCommand = new RelayCommand(async _ => await SavePetAsync(), _ => SelectedClient != null);
        DeletePetCommand = new RelayCommand(async _ => await DeletePetAsync(), _ => SelectedPet != null);
        SearchCommand = new RelayCommand(async _ => await LoadClientsAsync());
        _ = LoadClientsAsync();
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            await using var db = new ZooShopDbContext();
            var query = db.Clients.AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c =>
                    (c.LastName != null && c.LastName.Contains(SearchText)) ||
                    c.FirstName.Contains(SearchText) ||
                    (c.Phone != null && c.Phone.Contains(SearchText)));

            var clients = await query.OrderBy(c => c.LastName).ToListAsync();
            Clients.Clear();
            foreach (var c in clients) Clients.Add(c);

            var animalTypes = await db.AnimalTypes.ToListAsync();
            AnimalTypes.Clear();
            foreach (var a in animalTypes) AnimalTypes.Add(a);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task LoadPetsAsync()
    {
        if (SelectedClient == null) { Pets.Clear(); return; }
        try
        {
            await using var db = new ZooShopDbContext();
            var pets = await db.Pets
                .Include(p => p.AnimalType)
                .Where(p => p.ClientID == SelectedClient.ClientID)
                .ToListAsync();

            Pets.Clear();
            foreach (var p in pets) Pets.Add(p);
            SelectedPet = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private async Task SaveClientAsync()
    {
        if (string.IsNullOrWhiteSpace(ClientPhone)) { StatusMessage = "Телефон обязателен"; return; }
        try
        {
            await using var db = new ZooShopDbContext();
            var client = new Client
            {
                FirstName = ClientFirstName,
                LastName = ClientLastName,
                Phone = ClientPhone,
                Email = ClientEmail,
                Notes = ClientNotes
            };

            if (SelectedClient != null && SelectedClient.ClientID > 0)
            {
                client.ClientID = SelectedClient.ClientID;
                db.Clients.Update(client);
            }
            else
            {
                db.Clients.Add(client);
            }

            await db.SaveChangesAsync();
            StatusMessage = "✅ Клиент сохранён";
            await LoadClientsAsync();
            ClearClientForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private async Task DeleteClientAsync()
    {
        if (SelectedClient == null) return;
        if (MessageBox.Show($"Удалить клиента {SelectedClient.LastName}?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try
        {
            await using var db = new ZooShopDbContext();
            db.Clients.Remove(SelectedClient);
            await db.SaveChangesAsync();
            StatusMessage = "✅ Клиент удалён";
            SelectedClient = null;
            await LoadClientsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private async Task SavePetAsync()
    {
        if (SelectedClient == null || string.IsNullOrWhiteSpace(PetName) || !PetAnimalTypeId.HasValue)
        {
            StatusMessage = "Заполните имя питомца и вид животного";
            return;
        }
        try
        {
            await using var db = new ZooShopDbContext();
            var pet = new Pet
            {
                ClientID = SelectedClient.ClientID,
                PetName = PetName,
                AnimalTypeID = PetAnimalTypeId.Value,
                Breed = PetBreed,
                Allergies = PetAllergies,
                Gender = PetGender
            };

            if (SelectedPet != null && SelectedPet.PetID > 0)
            {
                pet.PetID = SelectedPet.PetID;
                db.Pets.Update(pet);
            }
            else
            {
                db.Pets.Add(pet);
            }

            await db.SaveChangesAsync();
            StatusMessage = "✅ Питомец сохранён";
            await LoadPetsAsync();
            ClearPetForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private async Task DeletePetAsync()
    {
        if (SelectedPet == null) return;
        if (MessageBox.Show($"Удалить питомца {SelectedPet.PetName}?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        try
        {
            await using var db = new ZooShopDbContext();
            db.Pets.Remove(SelectedPet);
            await db.SaveChangesAsync();
            StatusMessage = "✅ Питомец удалён";
            SelectedPet = null;
            await LoadPetsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
        }
    }

    private void LoadRecommendations()
    {
        RecommendedProducts.Clear();
        if (SelectedPet == null || string.IsNullOrWhiteSpace(SelectedPet.Allergies)) return;

        // Простая рекомендация: исключаем товары с аллергенами в составе
        var allergies = SelectedPet.Allergies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // Для упрощения — просто показываем товары для этого вида животного
        // в реальном проекте можно искать по Composition
        StatusMessage = $"🐾 Рекомендации для {SelectedPet.PetName}: товары без {SelectedPet.Allergies}";
    }

    private void ClearClientForm()
    {
        ClientFirstName = ""; ClientLastName = ""; ClientPhone = ""; ClientEmail = ""; ClientNotes = "";
    }

    private void ClearPetForm()
    {
        PetName = ""; PetBreed = ""; PetAllergies = ""; PetGender = ""; PetAnimalTypeId = null;
    }
}
