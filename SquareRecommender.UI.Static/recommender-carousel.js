$(document).ready(function () {
    const data = JSON.stringify({
        url: "https://my-business-107214.square.site/product/lenovo-flex/9",
    });

    var url =
        "https://squarerecommenderfunctions20210623230722.azurewebsites.net/api/merchant/MLG11XD6DZKCY/recommendations?code=uRhFSlSxxNQAiUDgEuiEdNz2s8criNZfWyFwKNu2/ZyDgFryaiouXg==";

    $.post(url, data)
        .done(function (data) {
            console.log(data);

            var content = `
                <div class='recommender' style='margin: 20px;height:600px;'>`;

            data.forEach((r) => {
                content += `
                  <div>
                      <a href="${r["page_url"]}">
                        <img src="${r["image_url"]}" style="width:400px;margin:auto;" />
                      </a>
                      <a href="${r["page_url"]}" style="text-decoration:none;color:black;font-size:25px;">
                        <p style="text-align:center"><strong>${r["name"]}</strong></p>
                      </a>
                  </div> 
                `;
            });

            content += `</div>`;

            $('.recommender-carousel').append(content);

            $(".recommender").slick({
                infinite: true,
                slidesToShow: 3,
                slidesToScroll: 3,
            });
        });
});