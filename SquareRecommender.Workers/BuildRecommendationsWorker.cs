using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using SquareRecommender.Db;
using SquareRecommender.Models;

namespace SquareRecommender.Workers
{
    public class BuildRecommendationsWorker
    {
        private readonly Datastore dataStore;
        private readonly ProductAssociationRepository productAssociationRepository;
        private readonly JobStatusRepository jobStatusRepository;
        private readonly ProductIndexRepository productIndexRepository;
        private readonly ProductRecommendationsRepository productRecommendationsRepository;
        private readonly MerchantRepository merchantRepository;
        private readonly MLContext context;
        private readonly string merchantId;

        public BuildRecommendationsWorker(string merchantId)
        {
            this.dataStore = new Datastore(merchantId);
            this.productAssociationRepository = new ProductAssociationRepository(this.dataStore);
            this.productIndexRepository = new ProductIndexRepository(this.dataStore);
            this.jobStatusRepository = new JobStatusRepository(this.dataStore);
            this.productRecommendationsRepository = new ProductRecommendationsRepository(this.dataStore);
            this.merchantRepository = new MerchantRepository();

            this.context = new MLContext();
            this.merchantId = merchantId;
        }

        public async Task Run()
        {
            await this.jobStatusRepository.AddMessage("Building product recommendations");

            var productAssociations = GetProductAssociations();

            var predictionEngine = CalculatePredictions(productAssociations);

            var productIndexes = GetProductIndexes();

            await BuildRecommendations(productIndexes, predictionEngine);

            await this.jobStatusRepository.AddMessage("Product recommendations are ready to use");

            UpdateMerchantRecommendationAvailability();
        }

        private List<ProductAssociation> GetProductAssociations()
        {
            var productAssociations = this.productAssociationRepository.GetAll();

            foreach (var productAssociation in productAssociations)
                productAssociation.Label = (float)Double.Parse(productAssociation.Id);

            return productAssociations;
        }

        private PredictionEngine<ProductAssociation, ProductPrediction> CalculatePredictions(List<ProductAssociation> productAssociations)
        {
            var trainData = this.context.Data.LoadFromEnumerable(productAssociations);

            var options = new MatrixFactorizationTrainer.Options()
            {
                MatrixColumnIndexColumnName = nameof(ProductAssociation.ProductId),
                MatrixRowIndexColumnName = nameof(ProductAssociation.CoPurchaseProductID),
                LabelColumnName = "Label",
                LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass,
                Alpha = 0.01,
                Lambda = 0.025,
            };

            var est = this.context.Recommendation().Trainers.MatrixFactorization(options);

            ITransformer model = est.Fit(trainData);

            return this.context.Model.CreatePredictionEngine<ProductAssociation, ProductPrediction>(model);
        }

        private List<ProductIndex> GetProductIndexes()
        {
            return this.productIndexRepository.GetAll();
        }

        private async Task BuildRecommendations(List<ProductIndex> productIndexes, PredictionEngine<ProductAssociation, ProductPrediction> predictionEngine)
        {
            var productIds = productIndexes.Select(p => p.Index);

            this.productRecommendationsRepository.RemoveAll();

            foreach (var productIndex in productIndexes)
            {
                var top6 = (from m in productIds
                            let p = predictionEngine.Predict(
                               new ProductAssociation()
                               {
                                   ProductId = productIndex.Index,
                                   CoPurchaseProductID = m
                               })
                            where m != productIndex.Index
                            orderby p.Score descending
                            select (m)).Take(6);

                var productRecommendations = from t in top6
                                             join p in productIndexes
                                             on t equals p.Index
                                             select p.ProductId;

                await productRecommendationsRepository.AddItem(new ProductRecommendations
                {
                    Id = productIndex.ProductId,
                    ProductId = productIndex.ProductId,
                    Recommendations = string.Join(",", productRecommendations)
                });
            }
        }

        private void UpdateMerchantRecommendationAvailability()
        {
            this.merchantRepository.UpdateRecommendationsAvailable(this.merchantId, true);
        }
    }
}
