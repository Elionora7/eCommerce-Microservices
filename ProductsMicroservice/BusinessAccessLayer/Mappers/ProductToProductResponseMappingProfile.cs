using AutoMapper;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.BusinessLogicLayer.DTO;


namespace eCommerce.BusinessLogicLayer.Mappers
{
    public class ProductToProductResponseMappingProfile : Profile
    {
        public ProductToProductResponseMappingProfile()
        {
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductID))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    ParseCategory(src.Category))) // Use helper method with error handling
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.QuantityInStock, opt => opt.MapFrom(src => src.QuantityInStock))
                .ForMember(dest => dest.imgUrl, opt => opt.MapFrom(src => src.imgUrl));
        }

        private CategoryOptions ParseCategory(string category)
        {
            // Case-insensitive parsing with error handling
            if (Enum.TryParse<CategoryOptions>(category, true, out var result))
            {
                return result;
            }

            // Log the error and provide a default or throw
            Console.WriteLine($"Warning: Could not parse category '{category}'");

            // You can either throw an exception or return a default value
            // throw new ArgumentException($"Invalid category: {category}");
            return CategoryOptions.Electronics; // Default fallback
        }
    }
}