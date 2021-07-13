using SquareRecommender.Db;
using SquareRecommender.Models;
using System;
using System.Collections.Generic;

namespace SquareRecommender.Workers
{
    public class JobStatusesGetWorker
    {
        private readonly JobStatusRepository jobStatusesRepository;

        public JobStatusesGetWorker(string merchantId)
        {
            var datastore = new Datastore(merchantId);
            this.jobStatusesRepository = new JobStatusRepository(datastore);
        }

        public List<JobStatus> Run()
        {
            return jobStatusesRepository.GetAll();
        }
    }
}
