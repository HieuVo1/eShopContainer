using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.AggregatesModel.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Configurations
{
    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders", OrderingDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            //builder.Ignore(b => b.DomainEvents);

            builder.Property(o => o.Id)
                .UseHiLo("orderseq", OrderingDbContext.DEFAULT_SCHEMA);

            builder
               .OwnsOne(o => o.Address, a =>
               {
                   // Explicit configuration of the shadow key property in the owned type 
                   // as a workaround for a documented issue in EF Core 5: https://github.com/dotnet/efcore/issues/20740
                   a.Property<int>("OrderId")
                   .UseHiLo("orderseq", OrderingDbContext.DEFAULT_SCHEMA);
                   a.WithOwner();
               });

            builder
               .Property<int?>("_buyerId")
               .UsePropertyAccessMode(PropertyAccessMode.Field)
               .HasColumnName("BuyerId")
               .IsRequired(false);

            builder
                .Property<DateTime>("_orderDate")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("OrderDate")
                    .IsRequired();

            builder
                .Property<int>("_orderStatusId")
                // .HasField("_orderStatusId")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("OrderStatusId")
                    .IsRequired();

            builder
                .Property<int?>("_paymentMethodId")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("PaymentMethodId")
                    .IsRequired(false);

            builder.Property<string>("Description").IsRequired(false);

            var navigation = builder.Metadata.FindNavigation(nameof(Order.OrderItems));

            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne<PaymentMethod>()
               .WithMany()
               // .HasForeignKey("PaymentMethodId")
               .HasForeignKey("_paymentMethodId")
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Buyer>()
                .WithMany()
                .IsRequired(false)
                // .HasForeignKey("BuyerId");
                .HasForeignKey("_buyerId");

            builder.HasOne(o => o.OrderStatus)
                .WithMany()
                // .HasForeignKey("OrderStatusId");
                .HasForeignKey("_orderStatusId");

        }
    }
}
