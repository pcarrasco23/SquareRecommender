var express = require("express");
const request = require("request");
const md5 = require("md5");
const messages = require("../messages");
const { ApiError, Client, Environment } = require("square");

var router = express.Router();

const { SQ_ENVIRONMENT, SQ_APPLICATION_SECRET, SQ_APPLICATION_ID } =
  process.env;

let environment;
if (SQ_ENVIRONMENT.toLowerCase() === "production") {
  environment = Environment.Production;
} else if (SQ_ENVIRONMENT.toLowerCase() === "sandbox") {
  environment = Environment.Sandbox;
} else {
  console.warn("Unsupported value for SQ_ENVIRONMENT in .env file.");
  process.exit(1);
}

// Configure Square defcault client
const squareClient = new Client({
  environment: environment,
});

// Configure Square OAuth API instance
const oauthInstance = squareClient.oAuthApi;

router.get("/", async function (req, res, next) {
  // Verify the state to protect against cross-site request forgery.
  if (req.cookies["Auth_State"] !== req.query["state"]) {
    content = messages.displayStateError();
    res.render("base", {
      content: content,
    });
  } else if (req.query["error"]) {
    // Check to see if the seller clicked the Deny button and handle it as a special case.
    if (
      "access_denied" === req.query["error"] &&
      "user_denied" === req.query["error_description"]
    ) {
      res.render(
        messages.displayError(
          "Authorization denied",
          "You chose to deny access to the app."
        )
      );
    }
    // Display the error and description for all other errors.
    else {
      content = messages.displayError(
        req.query["error"],
        req.query["error_description"]
      );
      res.render("base", {
        content: content,
      });
    }
  }
  // When the response_type is "code", the seller clicked Allow
  // and the authorization page returned the auth tokens.
  else if ("code" === req.query["response_type"]) {
    // Extract the returned authorization code from the URL
    var { code } = req.query;

    try {
      let { result } = await oauthInstance.obtainToken({
        // Provide the code in a request to the Obtain Token endpoint
        code,
        clientId: SQ_APPLICATION_ID,
        clientSecret: SQ_APPLICATION_SECRET,
        grantType: "authorization_code",
      });

      let {
        // Extract the returned access token from the ObtainTokenResponse object
        accessToken,
        refreshToken,
        expiresAt,
        merchantId,
      } = result;

      const merchantGetOptions = {
        url:
          "https://squarerecommenderfunctions20210623230722.azurewebsites.net/api/merchant/" +
          result.merchantId +
          "?code=uRhFSlSxxNQAiUDgEuiEdNz2s8criNZfWyFwKNu2/ZyDgFryaiouXg==",
        method: "GET",
      };

      request(merchantGetOptions, (err, merchantGetResult) => {
        if (merchantGetResult.statusCode == "404") {
          res.render("createaccount", {
            merchantId: result.merchantId,
            accessToken: result.accessToken,
            refreshToken: result.refreshToken,
          });
        } else if (err) {
          console.log(err);
          content = messages.displayError("Exception", JSON.stringify(err));
          res.render("base", {
            content: content,
          });
        } else {
          res.redirect(
            "/merchant?merchantId=" +
              result.merchantId +
              "&state=" +
              req.cookies["Auth_State"]
          );
        }
      });
    } catch (error) {
      // The response from the Obtain Token endpoint did not include an access token. Something went wrong.
      if (error instanceof ApiError) {
        content = messages.displayError(
          "Exception",
          JSON.stringify(error.result)
        );
        res.render("base", {
          content: content,
        });
      } else {
        console.log(error);
        content = messages.displayError("Exception", JSON.stringify(error));
        res.render("base", {
          content: content,
        });
      }
    }
  }
});

module.exports = router;
