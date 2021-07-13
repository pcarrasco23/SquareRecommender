using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Newtonsoft.Json.Linq;
using Square;
using Square.Exceptions;
using Square.Models;
using SquareRecommender.Db;
using SquareRecommender.Models;

namespace SquareRecommender.Workers
{
    public class CollectMerchantDataWorker
    {
        private readonly string merchantId;
        private readonly Datastore dataStore;
        private readonly MerchantRepository merchantRepository;
        private readonly ProductAssociationRepository productAssociationRepository;
        private readonly JobStatusRepository jobStatusRepository;
        private readonly ProductIndexRepository productIndexRepository;
        private readonly SquareClient squareClient;
        private IList<CatalogObject> squareCatalogItemVariants;
        private Dictionary<string, List<string>> productAssociations;
        private Dictionary<string, uint> productIndex;
        private uint productIndexMax = 0;

        public CollectMerchantDataWorker(string merchantId)
        {
            this.merchantId = merchantId;

            this.dataStore = new Datastore(merchantId);
            this.productAssociationRepository = new ProductAssociationRepository(this.dataStore);
            this.productIndexRepository = new ProductIndexRepository(this.dataStore);
            this.jobStatusRepository = new JobStatusRepository(this.dataStore);

            this.merchantRepository = new MerchantRepository();
            var merchant = merchantRepository.GetMerchant(merchantId);

            var squareEnvironment = System.Environment.GetEnvironmentVariable("SquareEnvironment");

            var env = squareEnvironment == "production" ? Square.Environment.Production : Square.Environment.Sandbox;

            this.squareClient = new SquareClient.Builder()
                .Environment(env)
                .AccessToken(merchant.AccessToken)
                .Build();

            this.productIndex = new Dictionary<string, uint>();
        }

        public async Task Run()
        {
            await this.jobStatusRepository.AddMessage("Processing product catalog and orders");

            await GetSquareItemVariants();

            if (this.squareCatalogItemVariants == null || this.squareCatalogItemVariants.Count == 0)
            {
                await this.jobStatusRepository.AddMessage("Catalog is empty.");

                return;
            }

            await GetSquareOrdersAsync();

            if (this.productAssociations == null || this.productAssociations.Count == 0)
            {
                await this.jobStatusRepository.AddMessage("There are no orders.");

                return;
            }

            await SaveProductAssociations();

            await SaveProductIndexes();

            await this.jobStatusRepository.AddMessage("Product catalog and orders processed");

            await TriggerRecommendationsBuilder();
        }

        private async Task GetSquareItemVariants()
        {
            try
            {
                var result = await squareClient.CatalogApi.ListCatalogAsync(types: "ITEM,ITEM_VARIATION");

                this.squareCatalogItemVariants = result.Objects;
            }
            catch (ApiException e)
            {
                Console.WriteLine("Failed to make the request");
                Console.WriteLine($"Response Code: {e.ResponseCode}");
                Console.WriteLine($"Exception: {e.Message}");
            }
        }

        private async Task GetSquareOrdersAsync()
        {
            var locations = await squareClient.LocationsApi.ListLocationsAsync();
            var locationIds = locations.Locations.Select(l => l.Id).ToList();

            var body = new SearchOrdersRequest.Builder()
              .LocationIds(locationIds)
              .Build();

            try
            {
                this.productAssociations = new Dictionary<string, List<string>>();

                var result = await squareClient.OrdersApi.SearchOrdersAsync(body: body);
                var orders = result.Orders;
                foreach (var order in orders)
                {
                    foreach (var lineItem in order.LineItems)
                    {
                        var itemVariant = squareCatalogItemVariants.FirstOrDefault(c => c.Id == lineItem.CatalogObjectId);
                        if (itemVariant != null)
                        {
                            var itemId = itemVariant.ItemVariationData.ItemId;
                            if (productAssociations.ContainsKey(itemId))
                            {
                                var coProducts = productAssociations[itemId];
                                foreach (var coLineItem in order.LineItems)
                                {
                                    if (coLineItem.CatalogObjectId != lineItem.CatalogObjectId)
                                    {
                                        var coItemVariant = squareCatalogItemVariants.FirstOrDefault(c => c.Id == coLineItem.CatalogObjectId);
                                        coProducts.Add(coItemVariant.ItemVariationData.ItemId);
                                    }

                                }
                            }
                            else
                            {
                                var coProducts = new List<string>();
                                foreach (var coLineItem in order.LineItems)
                                {
                                    if (coLineItem.CatalogObjectId != lineItem.CatalogObjectId)
                                    {
                                        var coItemVariant = squareCatalogItemVariants.FirstOrDefault(c => c.Id == coLineItem.CatalogObjectId);
                                        coProducts.Add(coItemVariant.ItemVariationData.ItemId);
                                    }

                                }
                                productAssociations.Add(itemId, coProducts);
                            }
                        }
                    }
                }
            }
            catch (ApiException e)
            {
                Console.WriteLine("Failed to make the request");
                Console.WriteLine($"Response Code: {e.ResponseCode}");
                Console.WriteLine($"Exception: {e.Message}");
            }
        }

        private async Task SaveProductAssociations()
        {
            this.productAssociationRepository.RemoveAll();

            try
            {
                int id = 1;
                foreach (var productAssociation in this.productAssociations)
                {
                    foreach (var coProduct in productAssociation.Value)
                    {
                        var productId = this.GetProductIndex(productAssociation.Key);
                        var coPurchaseProductId = this.GetProductIndex(coProduct);

                        await productAssociationRepository.AddItem(new ProductAssociation
                        {
                            Id = id.ToString(),
                            ProductId = productId,
                            CoPurchaseProductID = coPurchaseProductId
                        });

                        id++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to make the request " + e.Message);
            }
        }

        private uint GetProductIndex(string productId)
        {
            if (this.productIndex.ContainsKey(productId))
            {
                return this.productIndex[productId];
            }
            else
            {
                this.productIndex.Add(productId, ++this.productIndexMax);
                return this.productIndexMax;
            }
        }

        private async Task SaveProductIndexes()
        {
            this.productIndexRepository.RemoveAll();

            foreach (var productIndex in this.productIndex)
            {
                var response = await this.squareClient.CatalogApi.RetrieveCatalogObjectAsync(productIndex.Key, true);
                StreamReader reader = new StreamReader(response.Context.Response.RawBody);
                string body = reader.ReadToEnd();
                JObject json = JObject.Parse(body);
                var itemUri = json.SelectToken("$.object.item_data.ecom_uri");
                var itemName = json.SelectToken("$.object.item_data.name");
                var imageId = json.SelectToken("$.object.image_id");
                var itemImageUri = json.SelectToken("$.related_objects[?(@.id == '" + imageId + "')].image_data.url");

                await productIndexRepository.AddItem(new ProductIndex
                {
                    Id = productIndex.Key,
                    ProductId = productIndex.Key,
                    Index = productIndex.Value,
                    PageUrl = itemUri.ToString(),
                    ProductName = itemName.ToString(),
                    ImageUrl = itemImageUri.ToString()
                });
            }
        }

        private async Task TriggerRecommendationsBuilder()
        {
            string connectionString = System.Environment.GetEnvironmentVariable("SquareRecommenderQueue");

            var queue = new QueueClient(connectionString, "buildrecommendations", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await queue.SendMessageAsync(this.merchantId);
        }
    }
}
