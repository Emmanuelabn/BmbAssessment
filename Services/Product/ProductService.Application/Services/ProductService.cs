using System;
using System.Collections.Generic;
using System.Text;
using ProductService.Application.Mappers;
using ProductService.Application.Models;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var products = await _repository.GetAllAsync(cancellationToken);
            return products.Select(p => p.ToDto()).ToList();
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetByIdAsync(id, cancellationToken);
            return product?.ToDto();
        }

        public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = new Product();

            product.Name = request.Name;
            product.Price = request.Price;

            await _repository.AddAsync(product, cancellationToken);
            return product.ToDto();
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetByIdAsync(id, cancellationToken);
            if (product is null) return false;

            product.Name = request.Name;
            product.Price = request.Price;

            await _repository.UpdateAsync(product, cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetByIdAsync(id, cancellationToken);
            if (product is null) return false;

            await _repository.DeleteAsync(product, cancellationToken);
            return true;
        }
    }
}
