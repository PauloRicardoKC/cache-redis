using DemoCacheRedis.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoCacheRedis.Repositories.Interfaces
{
    public interface ICarRepository
    {
        Task<IEnumerable<Car>> GetCars();
    }
}
