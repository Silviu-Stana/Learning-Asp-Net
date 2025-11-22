using Models.DTO;

namespace Models.Interfaces
{
    public interface IStockService
    {
        Task<BuyOrderResponse> CreateBuyOrder(BuyOrderRequest? request);
        Task<SellOrderResponse> CreateSellOrder(SellOrderRequest? request);

        Task<List<BuyOrderResponse>> GetBuyOrders();
        Task<List<SellOrderResponse>> GetSellOrders();

    }
}
