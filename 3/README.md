# serverless-workshop
In this part, we will create a User Profile Lambda function. This function will talk to Auth0 and retrieve
information about the user. We will also set up an API Gateway. The API Gateway will allow our website to
invoke the function.
Lastly, we will create a custom authorizer. A custom authorizer is a special Lambda function that the API
Gateway executes to decide whether to allow or reject a request. We will use this custom authorizer to make
sure that only authenticated users have access to the User Profile Lambda function


##  1 Setup User Profile Lambda
- In the AWS console’s Services, click **Lambda** under **Compute**, and then click the **Create function** button.
- Click the **Author from scratch** button.
- **Name** the function **user-profile**.
- Set the **Runtime** to **C# (.NET Core 2.1)**
- Under **Role** select **Choose an existing role** and *lambda-s3-execution-role*.
- Click **Create function**. 
- Once the function is created, scroll down to the Basic settings section, set the **Timeout** to 0 minutes, 30
seconds. 
- Set **Hander** to **UserProfile::UserProfile.Function::FunctionHandler**
- Set the  **AUTH0_DOMAIN** and **ACCESS_TOKEN_PARAM_NAME** environment variables value to the ones you have from Auth0: 
- Set **AUTH_HEADER** environment variable to *Authorization*
- Click the **Save** button at the top of the page
- Now, to deploy a lambda, just run this command in Lambda's folder (`3\user-profile`):
```
dotnet lambda deploy-function --fn user-profile --region eu-west-1
``` 

## 2 Create API Gateway
- In the AWS Console’s Services tab, click on API Gateway under Networking & Content Delivery.
- If this is your first API Gateway, click the Get Started button. Otherwise, click the Create API button.
- Select the New API radio button.
- Type in a name for your API, such as **Workshop API** and optionally, a description.
- Set Endpoint Type to Regional
- Click Create API to create your API

## 3 Create Resource and Method
- In the Resources tab in the left navigation menu, select the Actions dropdown and click Create
Resource.
- Type “User Profile” in the Resource Name. The Resource Path should be automatically filled in.

   **Note**: Since the website will connect to the resource path /user-profile, this value must match
exactly.
- Tick the Enable API Gateway CORS box.
- Click the Create Resource button.
- The left-hand side list should now show /user-profile. Click it and then select the Actions dropdown again, and click the Create Method button to see a small dropdown under /user-profile. 
- Select GET  and click the button with the tick/check mark on it to confirm

After the Method, we need to configure the Integration Point:
- Click the Lambda Function radio button.
- Tick the checkbox labeled Use Lambda Proxy Integration.
- Select your region  from the Lambda Region dropdown.
- Type user-profile in the Lambda Function text box. 
- Click Save. 

## 4 Deploy
You should still be on your new API within API Gateway.
- Select the Actions dropdown again.
- Click Deploy API.
- In the Deploy API popup select [New Stage].
- Type **dev** as the Stage Name.
- Click Deploy to deploy the API.
- You will see will show the Invoke URL and a number of settings. Copy the Invoke URL as you will need it in the next step 

## 5 Update the website
We need to update the website to invoke the right API Gateway URL.
- Copy the config.js file containing your account specific settings, from the previous part (`2\website\js\config.js`) to the current site (`3/website/js/config.js`)
- Now edit the config.js file you just copied (`3/website/js/config.js`) to add the following line after
the auth0 section:
```
apiBaseUrl: 'API_GATEWAY_INVOKE_URL_FROM_STEP_4' 
```
- Deploy the site. Open	a	terminal/command-prompt	and	navigate	to	the	following	folder `3`
- Run (replace placeholder with site bucket name):
```
aws s3 sync .\website\ s3://YOUR_SITE_BUCKET_NAME --region eu-west-1
```

## 6 New Role
API Gateway supports custom request authorizers. These are Lambda functions that the API Gateways uses to
authorize requests. Custom authorizers can validate a token and return an IAM policy to authorize the request.
However, before we begin using custom authorizers we are going to create a different role for it.
- In the AWS console’s Services tab, click IAM under Security, Identity & Compliance and then click Roles
from the left navigation menu.
- Click Create role.
- From the Select role type step, select Lambda, and click the Next: Permissions button.
- From the list of policies check the box next to AWSLambdaBasicExecutionRole.
- Click Next: Review.
- Name the role api-gateway-lambda-exec-role.
- Click Create role to save and exit 

## 7 Custom Authorizer
Having created a new IAM role we can begin work on the custom authorizer now 

- In the AWS console’s Services tab, click Lambda under Compute, and then click Create function.
- Click the Author from scratch button.
- Name the function custom-authorizer.
- Under Runtime, select C# (.NET Core 2.1).
- Under Role, select Choose an existing role and your new role: api-gateway-lambda-exec-role. 
- Click Create function. 
- Click Environment variables to expand it, and create an environment variable with the key

   AUTH0_DOMAIN and set its value to the Auth0 domain
   AUTH0_AUDIENCE set to auth0 client id
- Under Basic settings:

   To improve the performance of your function, increase the Memory (MB) slider to 1536 MB. This will also allocate more CPU to your function.
   Set the Timeout to 0 minutes, 30 seconds.
- Set **Hander** to **CustomAuthorizer::CustomAuthorizer.Function::FunctionHandler**
- Click the Save button at the top of the page 
- Open `3\custom-authorizer` folder in console and deploy the function:
```
dotnet lambda deploy-function --fn custom-authorizer --region eu-west-1
```

## 8 Assign Custom Authorizer
Having deployed our custom authorizer function, we need to configure it so that it runs before our User Profile
function.
- In API Gateway open the API again.
- Click Authorizers from the left navigation menu.
- Click the Create New Authorizer button.
- Fill out the New Custom Authorizer form:
- Type in custom-authorizer as the name of the Lambda function.
- Set the region under Lambda Function to eu-west-1
- Set the Lambda Event Payload to Token.
- In the Token Source box, type 'Authorization'
- Click Create to create the custom authorizer.
- In the Add permission popup, click Grant & Create to confirm that you want to allow API Gateway to invoke the custom-authorizer function. 
- To make the custom authorizer invoke when the GET method is called, follow these steps:
- Click Resources in the left navigation menu.
- Click GET under /user-profile.
- Click Method Request.
- Click the pencil next to Authorization.
- From the dropdown select custom-authorizer and click the tick/check mark icon to save.

   Note: if custom-authorizer isn’t in the list, refresh the page
- Click on the HTTP Request Headers section to expand it.
- Click on the Add Header link, put “Authorization” for the name, and click the tick/check mark icon to the right and save it.
- Click on the URL Query String Parameters section to expand it.
- Click on the Add query string link, put “accessToken” for the name, and click the tick/check mark icon to the right and save it.
- Deploy the API again.
- Select the Actions dropdown.
- Click Deploy API.
- Select dev as the Deployment Stage.
- Click Deploy 

## 9 Test the system

- click the Sign Out button in the upper right.
- Refresh the page
- Log in to the website by clicking on Sign In button.
- Click the profile button (it’ll have your nickname and, possibly, your picture). After a short wait you
will see a modal box with your user information.
