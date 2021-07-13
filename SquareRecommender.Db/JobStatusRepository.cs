using SquareRecommender.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SquareRecommender.Db
{
    public class JobStatusRepository : Repository<JobStatus>
    {
        public JobStatusRepository(Datastore database) : base(database, "JobStatus")
        {
        }

        public async Task AddMessage(string message)
        {
            await AddItem(new JobStatus
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Message = message
            });
        }
    }
}
