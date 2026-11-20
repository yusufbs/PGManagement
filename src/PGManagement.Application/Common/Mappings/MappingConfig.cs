using Mapster;
using PGManagement.Contracts.Responses.Allocations;
using PGManagement.Contracts.Responses.Bills;
using PGManagement.Contracts.Responses.Payments;
using PGManagement.Contracts.Responses.Rooms;
using PGManagement.Contracts.Responses.Tenants;
using PGManagement.Domain.Entities;
using PGManagement.Domain.Enums;

namespace PGManagement.Application.Common.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Room, RoomResponse>.NewConfig()
            .Map(dest => dest.RoomType, src => src.RoomType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<Tenant, TenantResponse>.NewConfig()
            .Map(dest => dest.CurrentRoomNumber, src => 
                src.Allocations.FirstOrDefault(a => a.Status == AllocationStatus.Active) != null 
                    ? src.Allocations.First(a => a.Status == AllocationStatus.Active).Room.RoomNumber 
                    : null);

        TypeAdapterConfig<RoomAllocation, AllocationResponse>.NewConfig()
            .Map(dest => dest.RoomNumber, src => src.Room.RoomNumber)
            .Map(dest => dest.TenantName, src => $"{src.Tenant.FirstName} {src.Tenant.LastName}")
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<Bill, BillResponse>.NewConfig()
            .Map(dest => dest.TenantName, src => $"{src.RoomAllocation.Tenant.FirstName} {src.RoomAllocation.Tenant.LastName}")
            .Map(dest => dest.RoomNumber, src => src.RoomAllocation.Room.RoomNumber)
            .Map(dest => dest.PaidAmount, src => src.Payments.Sum(p => p.AmountPaid))
            .Map(dest => dest.BalanceDue, src => src.TotalAmount - src.Payments.Sum(p => p.AmountPaid))
            .Map(dest => dest.IsPaid, src => src.Payments.Sum(p => p.AmountPaid) >= src.TotalAmount);

        TypeAdapterConfig<Payment, PaymentResponse>.NewConfig()
            .Map(dest => dest.PaymentMode, src => src.PaymentMode.ToString());
    }
}
