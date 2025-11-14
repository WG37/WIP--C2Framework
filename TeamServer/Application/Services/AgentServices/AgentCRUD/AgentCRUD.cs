using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using TeamServer.Domain.Entities.Agents;
using TeamServer.Infrastructure.Data;

namespace TeamServer.Application.Services.AgentServices.AgentCRUD
{
    public class AgentCRUD : IAgentCRUD
    {
        private readonly AppDbContext _db;

        public AgentCRUD(AppDbContext db) => _db = db;

        // add
        public async Task AddAgentAsync(Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent), "Cannot add a null entity.");
            try
            {
                _db.Agents.Add(agent);
                await _db.SaveChangesAsync();
            }
            catch (DbException)
            {
                throw new DbUpdateException();
            }
        }

        // get all
        public async Task<IEnumerable<Agent>> GetAgentsAsync()
        {
            var agents = await _db.Agents.ToListAsync();
            if (agents == null)
                throw new InvalidOperationException("No entities exist in database.");

            return agents;
        }

        // get
        public async Task<Agent> GetAgentAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException($"The id: '{id}' is invalid.");

            var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Id == id);
            if (agent == null)
                throw new KeyNotFoundException("The id does not exist.");
            
            return agent;
        }

        // get uniqueId
        public async Task<Agent?> GetAgentByUniqueIdAsync(Guid uniqueId)
        {
            if (uniqueId == Guid.Empty)
                throw new ArgumentException($"The uniqueId: '{uniqueId}' is invalid.");

            return await _db.Agents
                .Include(a => a.Metadata)
                .FirstOrDefaultAsync(a => a.UniqueId == uniqueId);
        }

        // delete
        public async Task<bool> RemoveAgentAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException($"The id: '{id}' is invalid.");
            try
            {
                var agent = await GetAgentAsync(id);
                
                _db.Agents.Remove(agent);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            catch (DbException)
            {
                throw new DbUpdateException("Failed to save changes to database.");
            }

        }
    }
}
