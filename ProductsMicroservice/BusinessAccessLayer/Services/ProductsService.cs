using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.RepositoryContracts;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using FluentValidation.Results;
using System.Globalization;
using System.Linq.Expressions;

namespace eCommerce.BusinessLogicLayer.Services;

public class ProductsService : IProductsService
{
    private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IProductsRepository _productsRepository;
    private readonly IRabbitMQPublisher _rabbitMQPublisher;

    public ProductsService(IValidator<ProductAddRequest> productAddRequestValidator,
                           IValidator<ProductUpdateRequest> productUpdateRequestValidator, 
                           IMapper mapper, 
                           IProductsRepository productsRepository,
                           IRabbitMQPublisher rabbitMQPublisher)
    {
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
        _mapper = mapper;
        _productsRepository = productsRepository;
        _rabbitMQPublisher = rabbitMQPublisher;
    }


    public async Task<ProductResponse?> AddProduct(ProductAddRequest productAddRequest)
    {
        if (productAddRequest == null)
        {
            throw new ArgumentNullException(nameof(productAddRequest));
        }

        //Validate the product using Fluent Validation
        ValidationResult validationResult = await _productAddRequestValidator.ValidateAsync(productAddRequest);

        // Check the validation result
        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }


        //Attempt to add product
        //Map productAddRequest into 'Product' type
        //invokes ProductAddRequestToProductMappingProfile
        Product productInput = _mapper.Map<Product>(productAddRequest);                                                           
        Product? addedProduct = await _productsRepository.AddProduct(productInput);

        if (addedProduct == null)
        {
            return null;
        }

        //Map addedProduct into 'ProductRepsonse' type
        //(it invokes ProductToProductResponseMappingProfile)
        ProductResponse addedProductResponse = _mapper.Map<ProductResponse>(addedProduct);  

        return addedProductResponse;
    }


    public async Task<bool> DeleteProduct(Guid productID)
    {
        Product? existingProduct = await _productsRepository.GetProductByCondition(temp => temp.ProductID == productID);

        if (existingProduct == null)
        {
            return false;
        }

        //Attempt to delete product
        bool isDeleted = await _productsRepository.DeleteProduct(productID);
        
        if (isDeleted)
        {
            //Notify the ordersmicroservice that the product is deleted
            //string routingkey = "product.delete";
            
            var message = new ProductDeleteMessage(existingProduct.ProductID, existingProduct.ProductName
                );

            var headers = new Dictionary<string, object>()
              {
                { "event", "product.delete" },
                { "RowCount", 1 }
              };

            _rabbitMQPublisher.Publish(headers, message);
            // _rabbitMQPublisher.Publish<ProductDeleteMessage>(routingkey, message);
        }

        return isDeleted;
    }


    public async Task<ProductResponse?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        Product? product = await _productsRepository.GetProductByCondition(conditionExpression);
        if (product == null)
        {
            return null;
        }

        //Invokes ProductToProductResponseMappingProfile
        ProductResponse productResponse = _mapper.Map<ProductResponse>(product); 
        return productResponse;
    }


    public async Task<List<ProductResponse?>> GetProducts()
    {
        IEnumerable<Product?> products = await _productsRepository.GetProducts();

        //Invokes ProductToProductResponseMappingProfile
        IEnumerable<ProductResponse?> productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products); 
        return productResponses.ToList();
    }


    public async Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        IEnumerable<Product?> products = await _productsRepository.GetProductsByCondition(conditionExpression);
        //Invokes ProductToProductResponseMappingProfile
        IEnumerable<ProductResponse?> productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products); 
        return productResponses.ToList();
    }


    public async Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest)
    {
        Product? existingProduct = await _productsRepository.GetProductByCondition(temp => temp.ProductID == productUpdateRequest.ProductID);

        if (existingProduct == null)
        {
            throw new ArgumentException("Invalid Product ID");
        }


        //Validate the product using Fluent Validation
        ValidationResult validationResult = await _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

        // Check the validation result
        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage)); 
            throw new ArgumentException(errors);
        }


        //Map from ProductUpdateRequest to Product type
        //Invokes ProductUpdateRequestToProductMappingProfile
        Product product = _mapper.Map<Product>(productUpdateRequest); 


        //check if name is changed
        bool isNameChanged = productUpdateRequest.ProductName != existingProduct.ProductName;

        Product? updatedProduct = await _productsRepository.UpdateProduct(product);
        if (isNameChanged)
        {
            //string routingkey = "product.update.name";
            var message = new ProductNameUpdateMessage(product.ProductID, product.ProductName
                );

            var headers = new Dictionary<string, object>()
              {
                { "event", "product.update" },
                { "RowCount", 1 }
              };

            _rabbitMQPublisher.Publish(headers, message);
            //_rabbitMQPublisher.Publish< ProductNameUpdateMessage>(routingkey, message );
        }
        ProductResponse? updatedProductResponse = _mapper.Map<ProductResponse>(updatedProduct);

        return updatedProductResponse;
    }
}