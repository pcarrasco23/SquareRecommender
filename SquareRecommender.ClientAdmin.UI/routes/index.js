var express = require('express');
const { ApiError, Client, Environment } = require("square");
const md5 = require("md5");
var router = express.Router();

const { SQ_ENVIRONMENT, SQ_APPLICATION_ID } =
  process.env;

let basePath;
let environment;
if (SQ_ENVIRONMENT.toLowerCase() === "production") {
  basePath = `https://connect.squareup.com`;
  environment = Environment.Production;
} else if (SQ_ENVIRONMENT.toLowerCase() === "sandbox") {
  basePath = `https://connect.squareupsandbox.com`;
  environment = Environment.Sandbox;
} else {
  console.warn("Unsupported value for SQ_ENVIRONMENT in .env file.");
  process.exit(1);
}

const scopes = [
  "ITEMS_READ",
  "ORDERS_READ",
  "MERCHANT_PROFILE_READ",
  "ONLINE_STORE_SITE_READ",
  "ONLINE_STORE_SNIPPETS_READ",
  "ONLINE_STORE_SNIPPETS_WRITE",
];

router.get('/', function(req, res, next) {
  // Set the Auth_State cookie with a random md5 string to protect against cross-site request forgery.
  // Auth_State will expire in 300 seconds (5 mins) after the page is loaded.
  var state = md5(Date.now());
  var url =
    basePath +
    `/oauth2/authorize?client_id=${process.env.SQ_APPLICATION_ID}&` +
    `response_type=code&` +
    `scope=${scopes.join("+")}` +
    `&state=` +
    state;
  content = `
    <meta name="viewport" content="width=device-width">
    <div class="wrapper">
      <a class="btn"
       href="${url}">
         <strong>Connect</strong>
      </a>
    </div>`;
  res
    .cookie("Auth_State", state, { expire: Date.now() + 300000 })
    .render("index", {
      title: "Square Recommender App",
      content: content,
    });
});

module.exports = router;
