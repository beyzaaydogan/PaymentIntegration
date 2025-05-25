using System.Text.Json;
using AutoMapper;
using BalanceManagementApiClient;
using Microsoft.Extensions.Logging;
using PaymentIntegration.API.Models;
using PaymentIntegration.Application.Interfaces;
using PaymentIntegration.Domain.Entities;
using PaymentIntegration.Domain.Enums;
using PaymentIntegration.HttpClient;

namespace PaymentIntegration.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IBalanceManagementApiClient _balanceManagementApiClient;
    private readonly IPaymentRepository _paymentRepository;
    private IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentRepository paymentRepository, IBalanceManagementApiClient balanceManagementApiClient, IMapper mapper, ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _balanceManagementApiClient = balanceManagementApiClient;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<BaseResponse<string>> CreatePaymentAsync(CreatePreOrderRequest createPreOrderRequest)
    {
        try
        {
            var response = await _balanceManagementApiClient.PreorderAsync(new Body() { Amount = createPreOrderRequest.Amount, OrderId = createPreOrderRequest.OrderId });
            _logger.LogInformation($"Request successful! Response : {JsonSerializer.Serialize(response)}");
            
            var payment = _mapper.Map<Payment>(createPreOrderRequest);
        
            var paymentId = await _paymentRepository.CreateAsync(payment);
            
            return BaseResponse<string>.Ok($"Payment created successfully with id: {paymentId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}");
            return BaseResponse<string>.Fail(ex.Message);
        }
        
    }
    
    public async Task<BaseResponse<string>> CompletePaymentAsync(string orderId)
    {
        var result = await _paymentRepository.UpdatePaymentStatusAsync(orderId, PaymentStatus.Processing);
        
        if (!result)
            return BaseResponse<string>.Fail("Complete payment failed");
        
        try
        {
            var response = await _balanceManagementApiClient.CompleteAsync(new Body2() { OrderId = orderId });
            _logger.LogInformation($"Request successful! :  {JsonSerializer.Serialize(response)}");
            await _paymentRepository.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {JsonSerializer.Serialize(ex.Message)}");
            return BaseResponse<string>.Fail(ex.Message);
        }
        
        return BaseResponse<string>.Ok($"Payment for order: {orderId} completed successfully!");
    }
}
