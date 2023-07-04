using RedisExampleApp.API.Models;
using RedisExampleApp.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisExampleApp.API.Repositories
{
    public class ProductRepositoryWithCacheDecorator : IProductRepository
    {
        private const string productKey = "productCaches";
        private readonly IProductRepository _productrepository;
        private readonly RedisService _redisService;
        private readonly IDatabase _cacheRepository;
        public ProductRepositoryWithCacheDecorator(IProductRepository productrepository, RedisService redisService)
        {
            _productrepository=productrepository;
            _redisService=redisService;
            _cacheRepository=_redisService.GetDb(0);
        }

        public async Task<List<Product>> GetAsync()
        {
            if (!await _cacheRepository.KeyExistsAsync(productKey))
                return await LoadToCacheFromDbAsync();

            var products = new List<Product>();
            var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);
            foreach (var cacheProduct in cacheProducts.ToList())
            {
                var product = JsonSerializer.Deserialize<Product>(cacheProduct.Value);
                products.Add(product);
            }

            return products;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            if (await _cacheRepository.KeyExistsAsync(productKey))
            {
                var product = await _cacheRepository.HashGetAsync(productKey, id);
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;

            }

            var products = await LoadToCacheFromDbAsync();

            return products.FirstOrDefault(x => x.Id == id);

        }

        public async Task<Product> CreateAsync(Product product)
        {
            var newProduct = await _productrepository.CreateAsync(product);

            if (await _cacheRepository.KeyExistsAsync(productKey))
            {
                await _cacheRepository.HashSetAsync(productKey, product.Id, JsonSerializer.Serialize(newProduct));
            }
            return newProduct;

        }

        private async Task<List<Product>> LoadToCacheFromDbAsync()
        {
            var products = await _productrepository.GetAsync();

            products.ForEach(product =>
            {
                _cacheRepository.HashSet(productKey, product.Id, JsonSerializer.Serialize(product));
            });

            return products;
        }

    }
}
