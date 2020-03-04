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
    public class BrandRepository : IBrandRepository
    {
        private readonly IDistributedCache _cache;
        private const string Key_Brands = "Brands";

        public BrandRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        private IEnumerable<Brand> BrandsFromDatabase()
        {
            var count = 1;

            var list = new Faker<Brand>()
                .RuleFor(p => p.Id, p => count++)
                .RuleFor(p => p.Name, p => p.Vehicle.Manufacturer())
                .GenerateLazy(20);

            return list;
        }

        public async Task<IEnumerable<Brand>> GetBrands()
        {
            var cache = await _cache.GetStringAsync(Key_Brands);

            if (string.IsNullOrWhiteSpace(cache))
            {
                var cacheSettings = new DistributedCacheEntryOptions();
                cacheSettings.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                var brandsFromDatabase = BrandsFromDatabase();

                var itemsJson = JsonConvert.SerializeObject(brandsFromDatabase);

                await _cache.SetStringAsync(Key_Brands, itemsJson, cacheSettings);

                return await Task.FromResult(brandsFromDatabase);
            }

            var brandsFromCache = JsonConvert.DeserializeObject<IEnumerable<Brand>>(cache);

            return await Task.FromResult(brandsFromCache);
        }
    }
}
