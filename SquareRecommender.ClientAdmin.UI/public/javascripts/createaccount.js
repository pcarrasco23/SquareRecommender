var createAccount = function (merchantId, accessToken, refreshToken) {
  $("#create-account").attr("disabled", "disabled");
  $("#create-account").html(
    '<i class="fa fa-spinner fa-spin"></i> Create Account'
  );

  const url =
    "https://squarerecommenderfunctions20210623230722.azurewebsites.net/api/merchant?code=uRhFSlSxxNQAiUDgEuiEdNz2s8criNZfWyFwKNu2/ZyDgFryaiouXg==";
  const data = {
    merchantId: merchantId,
    accessToken: accessToken,
    refreshToken: refreshToken,
  };

  $.ajax({
    url: url,
    dataType: "text",
    type: "post",
    contentType: "application/json",
    data: JSON.stringify(data),
    success: function (res, textStatus, jQxhr) {
      window.location.href =
        "/merchant?merchantId=" +
        merchantId +
        "&state=" +
        Cookies.get("Auth_State");
    },
    error: function (jqXhr, textStatus, errRes) {
      console.log(errRes);
    },
  });
};
