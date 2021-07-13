function writeTokensOnSuccess(
  access_token,
  refresh_token,
  expires_at,
  merchant_id
) {
  return `
    <link type="text/css" rel="stylesheet" href="/stylesheets/style.css">
    <meta name="viewport" content="width=device-width">
    <div class="wrapper">
      <div class="messages">
        <h1>Authorization Succeeded</h1>
          <div style='color:rgba(204, 0, 35, 1)'><strong>Caution:</strong> NEVER store or share OAuth access tokens or refresh tokens in clear text.
                Use a strong encryption standard such as AES to encrypt OAuth tokens. Ensure the production encryption key is not
                accessible to anyone who does not need it.
          </div>
          <br/>
          <div><strong>OAuth access token:</strong> ${access_token} </div>
          <div><strong>OAuth access token expires at:</strong> ${expires_at} </div>
          <div><strong>OAuth refresh token:</strong> ${refresh_token} </div>
          <div><strong>Merchant Id:</strong> ${merchant_id} </div>
          <div><p>You can use this OAuth access token to call Create Payment and other APIs that were authorized by this seller.</p>
          <p>Try it out with <a href='https://developer.squareup.com/explorer/square/payments-api/create-payment' target='_blank'>API Explorer</a>.</p>
        </div>
    </div>
  </div>
  `;
}

// Display error message if the state doesn't match the state originally passed to the authorization page.
function displayStateError() {
  return ` 
      <link type="text/css" rel="stylesheet" href="/stylesheets/style.css">
      <meta name="viewport" content="width=device-width">
      <div class="wrapper">
        <div class="messages">
          <h1>Authorization failed</h1>
          <div>Invalid state parameter.</div>
        </div>
      </div>
    `;
}

// Display error if access token not acquired
function displayError(error, error_description) {
  return `
  <link type="text/css" rel="stylesheet" href="/stylesheets/style.css">
  <meta name="viewport" content="width=device-width">
  <div class="wrapper">
    <div class="messages">
      <h1>Authorization failed</h1>
      <div>Error: ${error} </div>
      <div>Error Details: ${error_description} </div>
    </div>
  </div>
  `;
}

function writeJobStatuses(jobStatuses) {
  return `
    <link type="text/css" rel="stylesheet" href="/stylesheets/style.css">
    <meta name="viewport" content="width=device-width">
    <div class="wrapper">
      <div class="messages">
        <h2>Initializing</h2>
          <br/>
          <p>
            ${jobStatuses}
          </p>
        </div>
    </div>
  </div>
  `;
}

function writeCarousel(recommendations) {
  var content = "";
  recommendations.forEach((r) => {
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

  return `
      <div class='recommender' style='margin: 20px;height:600px;'>
        ${content}
      </div>
      `;
}

module.exports = {
  writeTokensOnSuccess: writeTokensOnSuccess,
  displayStateError: displayStateError,
  displayError: displayError,
  writeJobStatuses: writeJobStatuses,
  writeCarousel: writeCarousel,
};
