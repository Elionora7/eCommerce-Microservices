using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;

public class OrdersService : IOrdersService
{
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly IMapper _mapper;
    private IOrdersRepository _ordersRepository;
    private UserMicroserviceClient _userMicroserviceClient;
    private ProductMicroserviceClient _productMicroserviceClient;


    public OrdersService(IOrdersRepository ordersRepository, 
        IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator,
        IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
        IValidator<OrderUpdateRequest> orderUpdateRequestValidator, 
        IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, 
        UserMicroserviceClient userMicroserviceClient,
        ProductMicroserviceClient productMicroserviceClient)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _mapper = mapper;
        _ordersRepository = ordersRepository;
        _userMicroserviceClient = userMicroserviceClient;
        _productMicroserviceClient = productMicroserviceClient;
    }


    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        if (orderAddRequest == null)
        {
            throw new ArgumentNullException(nameof(orderAddRequest));
        }


        //Validate OrderAddRequest using Fluent Validation
        ValidationResult orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(tmp => tmp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        List<ProductDTO?> products = new List<ProductDTO?>();

        //Validate orderitems using Fluent Validation
        foreach (OrderItemAddRequest _orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(_orderItemAddRequest);

            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }


            //TO DO: check if ProductId exist in Productmicroservice
            ProductDTO? product = await _productMicroserviceClient.GetProductByProductId(_orderItemAddRequest.ProductID);
            if (product == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }

            products.Add(product);
        }

        //TO DO: check if UserId exist in usermicroservice
        UserDTO? user = await _userMicroserviceClient.GetUserByUserId(orderAddRequest.UserID);
        if (user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }


        //Convert data from OrderAddRequest to Order
        //Map OrderAddRequest to Order
        Order order = _mapper.Map<Order>(orderAddRequest);


        //Generate values
        foreach (OrderItem orderItem in order.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        order.TotalBill = order.OrderItems.Sum(tmp => tmp.TotalPrice);


        //Invoke repository
        Order? addedOrder = await _ordersRepository.AddOrder(order);

        if (addedOrder == null)
        {
            return null;
        }

        //Map Order into OrderResponse type 
        OrderResponse orderResponse = _mapper.Map<OrderResponse>(addedOrder);

        //TO DO:add ProductName and Category to OrderItem
        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.Where(tmp => tmp.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }



        //TO DO: add UserName and Email from UsersMicroservice
        if (orderResponse != null)
        {
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }

        return orderResponse;
    }



    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        List<ProductDTO> products = new List<ProductDTO>();
        //Check for null parameter
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }


        //Validate OrderUpdateRequest using Fluent Validation
        ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        

        //Validate order items using Fluent Validation
        foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }


            //TO DO: call getproductbyproductid
            ProductDTO productDTO = await _productMicroserviceClient.GetProductByProductId(orderItemUpdateRequest.ProductID);
            if (productDTO == null)
            {
                throw new ArgumentException($"Product with ID {orderItemUpdateRequest.ProductID} does not exist", nameof(orderItemUpdateRequest.ProductID));
            }

            products.Add(productDTO);
        }

        //TO DO:checking if UserID exists in Usersmicroservice
        UserDTO userDTO = await _userMicroserviceClient.GetUserByUserId(orderUpdateRequest.UserID);
        if (userDTO == null)
        {
            throw new ArgumentException("Invalid User ID!");
        }


        //Map OrderUpdateRequest to Order type 
        Order order = _mapper.Map<Order>(orderUpdateRequest);
        //Generate values
        foreach (OrderItem orderItem in order.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
    order.TotalBill = order.OrderItems.Sum(temp => temp.TotalPrice);


        //Invoke repository
        Order? updatedOrder = await _ordersRepository.UpdateOrder(order);

        if (updatedOrder == null)
        {
            return null;
        }
        //Map Order into OrderResponse type (invokes OrderToOrderResponseMappingProfile)
        OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder); //Map updatedOrder ('Order' type) into 'OrderResponse' type (it invokes OrderToOrderResponseMappingProfile).


        //TO DO: Load ProductName and Category in OrderItem
        if (updatedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.Where(tmp => tmp.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }


        //Map user into OrderResponse type 
        if (updatedOrderResponse != null)
        {
            if (userDTO != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(userDTO, updatedOrderResponse);
            }
        }

        return updatedOrderResponse;
    }


    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder = await _ordersRepository.GetOrderByCondition(filter);

        if (existingOrder == null)
        {
            return false;
        }


        bool isDeleted = await _ordersRepository.DeleteOrder(orderID);
        return isDeleted;
    }


    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByCondition(filter);
        if (order == null)
            return null;

        OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);


        //TO DO: add ProductName and Category to OrderItem
        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productMicroserviceClient.GetProductByProductId(orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }


        //TO DO: Load User Name and Email from Users Microservice
        if (orderResponse != null)
        {
            UserDTO? user = await _userMicroserviceClient.GetUserByUserId(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }

        return orderResponse;
    }


    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByCondition(filter);


        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);


        //TO DO: add ProductName and Category in each OrderItem
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productMicroserviceClient.GetProductByProductId(orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }


            //TO DO: Load UserName and Email from UserMicroservice
            UserDTO? user = await _userMicroserviceClient.GetUserByUserId(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }

        return orderResponses.ToList();
    }


    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrders();

        IEnumerable<OrderResponse?> ordersResponse = _mapper.Map<IEnumerable<OrderResponse>>(orders);


        //TO DO: Load ProductName and Category in each OrderItem
        foreach (OrderResponse? orderResponse in ordersResponse)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productMicroserviceClient.GetProductByProductId(orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }


            //TO DO: Load User Name and Email from UserMicroservice
            UserDTO? user = await _userMicroserviceClient.GetUserByUserId(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }


        return ordersResponse.ToList();
    }
}