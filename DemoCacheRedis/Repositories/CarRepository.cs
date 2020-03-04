using Bogus;
using DemoCacheRedis.Models;
using DemoCacheRedis.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoCacheRedis.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly IDistributedCache _cache;
        private const string Key_Cars = "Cars";

        public CarRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        private IEnumerable<Car> CarsFromDatabase()
        {
            var count = 1;

            var list = new Faker<Car>()
                .RuleFor(p => p.Id, p => count++)
                .RuleFor(p => p.Model, p => p.Vehicle.Model())
                .RuleFor(p => p.Type, p => p.Vehicle.Type())
                .RuleFor(p => p.Fuel, p => p.Vehicle.Fuel())
                .GenerateLazy(50);

            return list;
        }

        public async Task<IEnumerable<Car>> GetCars()
        {
            var cache = await _cache.GetStringAsync(Key_Cars);

            if (string.IsNullOrWhiteSpace(cache))
            {
                var cacheSettings = new DistributedCacheEntryOptions();
                cacheSettings.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                var carsFromDatabase = CarsFromDatabase();

                var itemsJson = JsonConvert.SerializeObject(carsFromDatabase);

                await _cache.SetStringAsync(Key_Cars, itemsJson, cacheSettings);

                return await Task.FromResult(carsFromDatabase);
            }

            var carsFromCache = JsonConvert.DeserializeObject<IEnumerable<Car>>(cache);

            return await Task.FromResult(carsFromCache);
        }
    }
}