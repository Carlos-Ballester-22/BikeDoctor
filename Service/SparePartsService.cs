namespace BikeDoctor.Service;

using BikeDoctor.Models;
using BikeDoctor.Repository;
using System.Threading.Tasks;

public class SparePartsService : GenericService<SpareParts, Guid>, ISparePartsService
{
    public SparePartsService(ISparePartsRepository repository) : base(repository)
    {
    }

    public async Task<IEnumerable<SpareParts>> GetAllByEmployeeCIAsync(
        int employeeCI,
        int pageNumber = 1,
        int pageSize = 10
    )
    {
        return await ((ISparePartsRepository)_repository).GetAllByEmployeeCIAsync(employeeCI, pageNumber, pageSize);
    }

    public override async Task AddAsync(SpareParts spareParts)
    {
        ValidateSpareParts(spareParts);
        await base.AddAsync(spareParts);
    }

    public override async Task UpdateAsync(Guid id, SpareParts spareParts)
    {
        ValidateSpareParts(spareParts);        
        await base.UpdateAsync(id, spareParts);
    }

    private void ValidateSpareParts(SpareParts spareParts)
    {
        if (spareParts.ClientCI <= 0)
            throw new ArgumentException("El CI del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(spareParts.MotorcycleLicensePlate))
            throw new ArgumentException("La placa de la motocicleta es obligatoria.");
        if (spareParts.EmployeeCI <= 0)
            throw new ArgumentException("El CI del empleado es obligatorio.");
    }
}