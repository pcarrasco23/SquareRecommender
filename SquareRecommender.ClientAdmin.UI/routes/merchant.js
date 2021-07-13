var express = require("express");
const request = require("request");
const messages = require("../messages");

var router = express.Router();

router.get("/", async function (req, res, next) {
  // Verify the state to protect against cross-site request forgery.
  if (req.cookies["Auth_State"] !== req.query["state"]) {
    content = messages.displayStateError();
    res.render("index", {
      content: content,
    });
  }

  const merchantGetOptions = {
    url:
      "https://squarerecommenderfunctions20210623230722.azurewebsites.net/api/merchant/" +
      req.query["merchantId"] +
      "?code=uRhFSlSxxNQAiUDgEuiEdNz2s8criNZfWyFwKNu2/ZyDgFryaiouXg==",
    method: "GET",
  };

  request(merchantGetOptions, (err, merchantGetResult) => {
    if (err) {
      console.log(err);
      content = messages.displayError("Exception", JSON.stringify(err));
      res.render("index", {
        content: content,
      });
    }

    const jobStatusGetOptions = {
      url:
        "https://squarerecommenderfunctions20210623230722.azurewebsites.net/api/merchant/" +
        req.query["merchantId"] +
        "/jobstatuses" +
        "?code=uRhFSlSxxNQAiUDgEuiEdNz2s8criNZfWyFwKNu2/ZyDgFryaiouXg==",
      method: "GET",
    };

    request(jobStatusGetOptions, (err, jobStatusesGetResult) => {
      console.log(jobStatusesGetResult.body);

      if (err) {
        console.log(err);
        content = messages.displayError("Exception", JSON.stringify(err));
        res.render("index", {
          content: content,
        });
      }

      const merchantData = JSON.parse(merchantGetResult.body);
      const jobStatuses = JSON.parse(jobStatusesGetResult.body);
      if (merchantData.recommendationsAvailable) {
        const message = `
                    <p>Product recommendations have been build</p>
                `;
        res.render("merchant", {
          message: message,
          merchantId: req.query["merchantId"],
          jobStatuses: jobStatuses,
        });
      } else {
        const message = `
                    <p>Your account has been created and product recommendations are being build. Refresh to see the progress.</p>
                `;
        res.render("merchant", {
          message: message,
          merchantId: req.query["merchantId"],
          jobStatuses: jobStatuses,
        });
      }
    });
  });
});

module.exports = router;
