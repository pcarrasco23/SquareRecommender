using SquareRecommender.Models;
using SquareRecommender.Db;
using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Square;
using Square.Models;
using Square.Exceptions;

namespace SquareRecommender.Workers
{
    public class MerchantAddWorker
    {
        private readonly dynamic requestData;
        private readonly SquareClient squareClient;
        private string merchantSiteDomain;
        public MerchantAddWorker(dynamic requestData)
        {
            this.requestData = requestData;

            var squareEnvironment = System.Environment.GetEnvironmentVariable("SquareEnvironment");

            var env = squareEnvironment == "production" ? Square.Environment.Production : Square.Environment.Sandbox;

            this.squareClient = new SquareClient.Builder()
               .Environment(env)
               .AccessToken((string)requestData.accessToken)
               .Build();
        }

        public async Task Run()
        {
            await GetMerchantSite();

            await AddMerchantRecord();

            await AddSnippet();

            await TriggerCollectMerchantData();
        }

        private async Task GetMerchantSite()
        {
            var listSitesResult = await this.squareClient.SitesApi.ListSitesAsync();

            if (listSitesResult.Sites != null && listSitesResult.Sites.Count > 0)
            {
                this.merchantSiteDomain = listSitesResult.Sites[0].Domain.ToLower();
            }
        }

        private async Task AddMerchantRecord()
        {
            var merchantRepository = new MerchantRepository();

            var merchant = merchantRepository.GetMerchant((string)requestData.merchantId);
            if (merchant != null)
            {
                var clientId = System.Environment.GetEnvironmentVariable("SquareApplicationId");
                var secret = System.Environment.GetEnvironmentVariable("SquareClientSecret");

                await this.squareClient.OAuthApi.RevokeTokenAsync(new RevokeTokenRequest(clientId, merchant.AccessToken, null, true), secret);

                merchantRepository.RemoveItem(merchant.Id);
            }

            await merchantRepository.AddItem(new Models.Merchant
            {
                Id = Guid.NewGuid().ToString(),
                MerchantId = requestData.merchantId,
                AccessToken = requestData.accessToken,
                RefreshToken = requestData.refreshToken,
                Domain = this.merchantSiteDomain,
                Enabled = true,
                RecommendationsAvailable = false
            });
        }

        private async Task AddSnippet()
        {
            var listSitesResult = await squareClient.SitesApi.ListSitesAsync();

            foreach (var site in listSitesResult.Sites)
            {
                var snippet = new Snippet("<script src =\"https://code.jquery.com/jquery-3.6.0.min.js\"             integrity=\"sha256-/xUj+3OJU5yExlq6GSYGSHk7tPXikynS7ogEvDej/m4=\"             crossorigin=\"anonymous\"></script>     <script>         $(document).ready(function () {             if (window.location.href.indexOf('/product/') >= 0) {                 loadRecommendations(window.location.href);             }              document.addEventListener('viewproduct', function () {                 setTimeout(() => {                     var url = $(this).attr('href');                     loadRecommendations(window.location.href + url);                 }, 1000);             });         });          function loadRecommendations(productUrl) {             $('head').append('<link rel=\"stylesheet\" type=\"text/css\" href=\"https://squarerecommenderstorage.z13.web.core.windows.net/slick.css\" />');             $('head').append('<link rel=\"stylesheet\" type=\"text/css\" href=\"https://squarerecommenderstorage.z13.web.core.windows.net/slick-theme.css\" />');             $('head').append('<script type=\"text/javascript\" src=\"https://squarerecommenderstorage.z13.web.core.windows.net/slick.min.js\"/>');             $('head').append(`<style type=\"text/css\">                 .slider {                     width: 50 %;                     margin: 100px auto;                 }                  .slick - slide {                     margin: 0px 20px;                 }                  .slick - slide img {                     width: 100 %;                 }                  .slick - prev: before,                 .slick - next: before {                     color: black;                 }                  .slick - current {                     opacity: 1;                 }             </style >`);              var count = 0;             var intervalId = setInterval(() => {                 count++;                  if (count > 5) {                     clearInterval(intervalId);                 }                 else if ($('.user-content').length > 0) {                     var recommender = $('<div class=\"recommender-carousel container\" style=\"height: 450px;width: 100%;\"/>');                      var data = JSON.stringify({                         url: productUrl                     });                      var url =                         \"https://squarerecommenderfunctions20210623230722.azurewebsites.net/api/recommendations?code=uRhFSlSxxNQAiUDgEuiEdNz2s8criNZfWyFwKNu2/ZyDgFryaiouXg==\";                      $.post(url, data)                         .done(function (data) {                             if (data.length > 0) {                                 var content = `                                 <h2>Customers Also Bought</h2>                                 <div class='recommender'>`;                                  data.forEach((r) => {                                     content += `                                   <div>                                       <a href=\"${r[\"page_url\"]}\">                                         <img src=\"${r[\"image_url\"]}\" style=\"width:300px;margin:auto;\" />                                       </a>                                       <a href=\"${r[\"page_url\"]}\" style=\"text-decoration:none;color:black;font-size:25px;\">                                         <p style=\"text-align:center\"><strong>${r[\"name\"]}</strong></p>                                       </a>                                   </div>                                  `;                                 });                                  content += `</div>`;                                  recommender.append(content);                                  $('.recommender-carousel').remove();                                 $(recommender).insertAfter($('.user-content')[0]);                                  $(\".recommender\").slick({                                     infinite: true,                                     slidesToShow: 3,                                     slidesToScroll: 3,                                     prevArrow: '<div data-v-125ee3d0=\"\" class=\"carousel__arrow carousel__arrow--enabled\" style=\"position: absolute;top: 50%;margin-left: -75px\"><svg data-v-125ee3d0=\"\" width=\"24px\" height=\"24px\" xmlns=\"http://www.w3.org/2000/svg\" role=\"img\" class=\"icon\"><!----> <use xlink:href=\"#chevron-left-icon\" style=\"--background-fill:white; --icon-fill:currentColor;\"></use></svg></div>',                                     nextArrow: '<div data-v-125ee3d0=\"\" class=\"carousel__arrow carousel__arrow--enabled\" style=\"float: right;margin-top: -150px;margin-right: -75px\"><svg data-v-125ee3d0=\"\" width=\"24px\" height=\"24px\" xmlns=\"http://www.w3.org/2000/svg\" role=\"img\" class=\"icon\"><!----> <use xlink:href=\"#chevron-right-icon\" style=\"--background-fill:white; --icon-fill:currentColor;\"></use></svg></div>'                                 });                             }                         });                      clearInterval(intervalId);                 }             }, 1000);         }     </script>");
                var body = new UpsertSnippetRequest.Builder(snippet: snippet)
                    .Build();

                try
                {
                    var result = await squareClient.SnippetsApi.UpsertSnippetAsync(siteId: site.Id, body: body);
                }
                catch (ApiException e)
                {
                    Console.WriteLine("Failed to make the request");
                    Console.WriteLine($"Response Code: {e.ResponseCode}");
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }
        }

        private async Task TriggerCollectMerchantData()
        {
            string merchantId = requestData.merchantId;
            string connectionString = System.Environment.GetEnvironmentVariable("SquareRecommenderQueue");

            var queue = new QueueClient(connectionString, "collectmerchantdata", new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await queue.SendMessageAsync(merchantId);
        }
    }
}
