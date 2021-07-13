using Square;
using Square.Exceptions;
using SquareRecommender.Db;
using SquareRecommender.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SquareRecommender.Workers
{
    public class MerchantDeleteWorker
    {
        private readonly Datastore dataStore;
        private readonly MerchantRepository merchantRepository;
        private readonly string merchantId;
        private readonly Merchant merchant;

        public MerchantDeleteWorker(string merchantId)
        {
            this.merchantId = merchantId;
            this.dataStore = new Datastore(merchantId);

            this.merchantRepository = new MerchantRepository();
            this.merchant = merchantRepository.GetMerchant(merchantId);
        }

        public async Task Run()
        {
            await DeleteSnippet();

            await this.dataStore.RemoveMerchantData();

            this.merchantRepository.RemoveItem(merchant.Id);
        }

        private async Task DeleteSnippet()
        {
            var squareEnvironment = System.Environment.GetEnvironmentVariable("SquareEnvironment");

            var env = squareEnvironment == "production" ? Square.Environment.Production : Square.Environment.Sandbox;

            var squareClient = new SquareClient.Builder()
               .Environment(env)
               .AccessToken(this.merchant.AccessToken)
               .Build();

            var listSitesResult = await squareClient.SitesApi.ListSitesAsync();

            foreach (var site in listSitesResult.Sites)
            {
                try
                {
                    await squareClient.SnippetsApi.DeleteSnippetAsync(siteId: site.Id);
                }
                catch (ApiException e)
                {
                    Console.WriteLine("Failed to make the request");
                    Console.WriteLine($"Response Code: {e.ResponseCode}");
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }
        }
    }
}
