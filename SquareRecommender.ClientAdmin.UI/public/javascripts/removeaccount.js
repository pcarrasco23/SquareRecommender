var removeAccount = function (merchantId) {
  $("#remove-account").attr("disabled", "disabled");
  $("#remove-account").html(
    '<i class="fa fa-spinner fa-spin"></i> Remove Account'
  );

  const url =
    "https://squarerecommenderfunctions20210623230722.azurewebsites.net/api/merchant/" +
    merchantId +
    "?code=uRhFSlSxxNQAiUDgEuiEdNz2s8criNZfWyFwKNu2/ZyDgFryaiouXg==";

  $.ajax({
    url: url,
    type: "delete",
    success: function (res, textStatus, jQxhr) {
      window.location.href = "/";
    },
    error: function (jqXhr, textStatus, errRes) {
      console.log(errRes);
    },
  });
};
