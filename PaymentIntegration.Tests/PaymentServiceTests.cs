using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentIntegration.API.Models;
using PaymentIntegration.Application.Interfaces;
using PaymentIntegration.Domain.Entities;
using PaymentIntegration.Domain.Enums;
using PaymentIntegration.HttpClient;
using PaymentIntegration.Infrastructure.Services;

namespace PaymentIntegration.Tests;

public class PaymentServiceTests
{
    private readonly Mock<IBalanceManagementApiClient> _balanceClientMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _paymentService;
    
    public PaymentServiceTests()
    {
        _balanceClientMock = new Mock<IBalanceManagementApiClient>();
        _mapperMock = new Mock<IMapper>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _loggerMock = new Mock<ILogger<PaymentService>>();

        _paymentService = new PaymentService(
            _paymentRepositoryMock.Object,
            _balanceClientMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }
     

    [Fact]
    public async Task CreatePaymentAsync_ReturnsSuccess_WhenPreorderSucceeds()
    {
        // Arrange
        var createPreOrderRequest = new CreatePreOrderRequest
        {
            Amount = 100,
            OrderId = "order123"
        };

        var paymentEntity = new Payment { };

        var paymentId = "payment123";

        _balanceClientMock
            .Setup(x => x.PreorderAsync(It.IsAny<Body>()))
            .ReturnsAsync(It.IsAny<Response3>());

        _mapperMock
            .Setup(m => m.Map<Payment>(createPreOrderRequest))
            .Returns(paymentEntity);

        _paymentRepositoryMock
            .Setup(r => r.CreateAsync(paymentEntity))
            .ReturnsAsync(paymentId);

        // Act
        var result = await _paymentService.CreatePaymentAsync(createPreOrderRequest);

        // Assert
        Assert.True(result.Success);
        Assert.Contains(paymentId, result.Data);
        
        _balanceClientMock.Verify(x => x.PreorderAsync(It.Is<Body>(b => b.Amount == 100 && b.OrderId == "order123")), Times.Once);
        _mapperMock.Verify(m => m.Map<Payment>(createPreOrderRequest), Times.Once);
        _paymentRepositoryMock.Verify(r => r.CreateAsync(paymentEntity), Times.Once);
    }
        
    
    [Fact]
    public async Task CreatePaymentAsync_ReturnsFailure_WhenPreorderThrowsException()
    {
        // Arrange
        var createPreOrderRequest = new CreatePreOrderRequest
        {
            Amount = 100,
            OrderId = "order123"
        };

        var exceptionMessage = "API failure";

        _balanceClientMock
            .Setup(x => x.PreorderAsync(It.IsAny<Body>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _paymentService.CreatePaymentAsync(createPreOrderRequest);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(exceptionMessage, result.Error);

        _balanceClientMock.Verify(x => x.PreorderAsync(It.IsAny<Body>()), Times.Once);
        _mapperMock.Verify(m => m.Map<Payment>(It.IsAny<CreatePreOrderRequest>()), Times.Never);
        _paymentRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task CompletePaymentAsync_ReturnsFailure_WhenUpdatePaymentStatusFails()
    {
        // Arrange
        var orderId = "order123";
        _paymentRepositoryMock
            .Setup(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Processing))
            .ReturnsAsync(false);

        // Act
        var result = await _paymentService.CompletePaymentAsync(orderId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Complete payment failed", result.Error);

        _paymentRepositoryMock.Verify(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Processing), Times.Once);
        _balanceClientMock.Verify(c => c.CompleteAsync(It.IsAny<Body2>()), Times.Never);
        _paymentRepositoryMock.Verify(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed), Times.Never);
    }

    [Fact]
    public async Task CompletePaymentAsync_ReturnsFailure_WhenCompleteAsyncThrowsException()
    {
        // Arrange
        var orderId = "order123";
        var exceptionMessage = "API error";

        _paymentRepositoryMock
            .Setup(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Processing))
            .ReturnsAsync(true);

        _balanceClientMock
            .Setup(c => c.CompleteAsync(It.IsAny<Body2>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _paymentService.CompletePaymentAsync(orderId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(exceptionMessage, result.Error);

        _paymentRepositoryMock.Verify(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Processing), Times.Once);
        _balanceClientMock.Verify(c => c.CompleteAsync(It.Is<Body2>(b => b.OrderId == orderId)), Times.Once);
        _paymentRepositoryMock.Verify(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed), Times.Never);
    }

    [Fact]
    public async Task CompletePaymentAsync_ReturnsSuccess_WhenPaymentCompletesSuccessfully()
    {
        // Arrange
        var orderId = "order123";
        var responseMock = new Response4();

        _paymentRepositoryMock
            .Setup(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Processing))
            .ReturnsAsync(true);

        _balanceClientMock
            .Setup(c => c.CompleteAsync(It.IsAny<Body2>()))
            .ReturnsAsync(responseMock);

        _paymentRepositoryMock
            .Setup(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed))
            .ReturnsAsync(true);

        // Act
        var result = await _paymentService.CompletePaymentAsync(orderId);

        // Assert
        Assert.True(result.Success);
        Assert.Contains(orderId, result.Data);

        _paymentRepositoryMock.Verify(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Processing), Times.Once);
        _balanceClientMock.Verify(c => c.CompleteAsync(It.Is<Body2>(b => b.OrderId == orderId)), Times.Once);
        _paymentRepositoryMock.Verify(r => r.UpdatePaymentStatusAsync(orderId, PaymentStatus.Completed), Times.Once);
    }

}
