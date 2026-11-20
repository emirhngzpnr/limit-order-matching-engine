using MatchEngine.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatchEngine.Persistence
{
    public class SqlOrderCommandStore
    {
        private readonly MatchEngineDbContext _context;
        public SqlOrderCommandStore(MatchEngineDbContext context)
        {
            _context = context;
        }
        
        public async Task AppendAsync(OrderCommand command)
        {
            OrderCommandEntity entity;
            switch (command)
            {
                case SubmitOrderCommand submit:
                    entity = OrderCommandMapper.FromSubmit(submit);
                    break;

                case CancelOrderCommand cancel:
                    entity = OrderCommandMapper.FromCancel(cancel);
                    break;

                default:
                    throw new ArgumentException(
                        "Desteklenmeyen komut türü.",
                        nameof(command));
            }
            _context.OrderCommands.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<StoredOrderCommand>> ReadBatchAsync(
                                        long afterSequenceNumber,
                                        int batchSize)
        {
            if (afterSequenceNumber < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(afterSequenceNumber));
            }
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(batchSize));
            }

            var entities = await _context.OrderCommands
                .AsNoTracking()
                .Where(entity =>
                        entity.SequenceNumber > afterSequenceNumber)
                .OrderBy(entity => entity.SequenceNumber)
                .Take(batchSize)
                .ToListAsync();

            return entities
                .Select(entity => new StoredOrderCommand(
                    entity.SequenceNumber,
                    OrderCommandMapper.ToCommand(entity)))
                .ToArray();
                
        }
    }
}
