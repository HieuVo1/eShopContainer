﻿using Microsoft.EntityFrameworkCore;
using Ordering.Domain.AggregatesModel.BuyerAggregate;
using Ordering.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Repositories
{
    public class BuyerRepository : IBuyerRepository
    {
        private readonly OrderingDbContext _context;
        public BuyerRepository(OrderingDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); ;
        }

        public IUnitOfWork UnitOfWork => _context;

        public Buyer Add(Buyer buyer)
        {
            if (buyer.IsTransient())
            {
                return _context.Buyers
                    .Add(buyer)
                    .Entity;
            }

            return buyer;
        }

        public async Task<Buyer> FindAsync(string buyerIdentityGuid)
        {
            var buyer = await _context.Buyers
               .Include(b => b.PaymentMethods)
               .Where(b => b.IdentityGuid == buyerIdentityGuid)
               .SingleOrDefaultAsync();

            return buyer;
        }

        public async Task<Buyer> FindByIdAsync(string id)
        {
            var buyer = await _context.Buyers
                .Include(b => b.PaymentMethods)
                .Where(b => b.Id == int.Parse(id))
                .SingleOrDefaultAsync();

            return buyer;
        }

        public Buyer Update(Buyer buyer)
        {
            return _context.Buyers
              .Update(buyer)
              .Entity;
        }
    }
}
