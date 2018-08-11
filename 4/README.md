# serverless-workshop
In this part, we are going to add the ability to upload videos from the browser to your S3 bucket


## 1 Create lambda function for reading policy

You will need to create a new Lambda function in the AWS console. This Lambda function will generate a
policy document to allow your users upload videos to S3. To create the lambda function:
- Go to Lambda in the AWS Console.
- Create a new function as you did for your other Lambda functions.
- Name the function get-upload-policy and select C# (.NET Core 2.1) as the Runtime.
- Assign the lambda-s3-execution-role YOU CREATED policy to it (the same policy created in part 1).
- Set the Timeout to 30 seconds.
- Set handler to GetUploadPolicy::GetUploadPolicy.Function::FunctionHandler
- Leave all other settings as their default values and Save the function 

## 2 Create IAM user

The policy and credentials that we are going to generate in the Lambda function need to be signed by an IAM
user that has permissions to upload files to S3. Let’s create this user now.
- Go to IAM in the AWS console.
- Click Users in the left navigation menu, then click the Add user button in the top left.
- Set the username to upload-s3 and check the box labelled Programmatic access 
- Click Next: Permissions and then skip adding permissions at this time by clicking Next: Review.
- Ignore the warning that This user has no permissions and click Create user.
- You will then be shown the following screen where you must download the user’s Access key id and Secret access key as a CSV: click the Download .csv button and save the credentials.csv file to your computer.
- Click the Close button. 
- Click the upload-s3 user and click the Permissions tab.
- Click Add inline policy 
- Click on the JSON tab
• Copy the content of file `4\ima-user.json` to the Policy Document and save (make sure to specify your upload bucket name
in the policy) 
- Click on the Review policy button
- Set the name of the policy to upload-policy.
- Click Create policy 

## 3 Configure and deploy function
Now we need to configure and deploy the function to AWS.
- In the AWS console click Lambda.
- Click get-upload-policy in the function list.
- Create two environment variables with the keys ACCESS_KEY_ID and SECRET_ACCESS_KEY. The
values of these variables will be in the .csv file you downloaded in step 2 which you need to copy and
paste into this screen.
- Create a third environment variable UPLOAD_BUCKET and set it to your video upload S3 bucket (workshop-upload).
- Scroll to the top of the page and click the Save button. 
- Open `4\get-upload-policy` folder in console and deploy the function:
```
dotnet lambda deploy-function --fn get-upload-policy --region eu-west-1
```

## 4 Create Resource and Method in the API Gateway
In this step we will create a resource and a method in the API Gateway. We will use it to invoke the Lambda
function we deployed in the previous step.
- Go to API Gateway in the AWS console.
- Select our gateway
- Under Resources in the second column, ensure that the / resource is selected.
- Select Actions and then click Create Resource.
- Set the Resource Name to s3-policy-document.
- Ensure that the Enable API Gateway CORS box is checked 
- Click Create Resource.
- Make sure that s3-policy-document is selected under Resources and click Actions.
- Click Create Method.
- From the dropdown box under the resource name, select GET and click the tick/check mark button to
save.
- In the screen that appears select Lambda Function radio
- Check the checkbox with the label Use Lambda Proxy Integration
- Set eu-west-1 as the Lambda Region
- Type get-upload-policy in Lambda Function textbox
- Click Save. Click OK in the dialog box that appears. 

To make the custom authorizer invoke on the GET method, follow these steps:
- Ensure you are still on the Resources tab in the left navigation menu.
- Click GET under /s3-policy-document in the second column.
- Click Method Request in the /s3-policy-document - GET - Method Execution section.
- Click the pencil next to Authorization.
- From the dropdown select custom authorizer and the little tick next to it.
- Click on the URL Query String Parameters section to expand it.
- Click on the Add query string link, put “accessToken” for the name, and click the tick/check mark
icon to the right and save it.
- Click on the HTTP Request Headers section to expand it.
- Click on the Add Header link, put “Authorization” for the name, and click the tick/check mark icon
to the right to save it 

## 5 Deploy API Gateway
Finally, we need to deploy the API so that our changes go live.
- Click Actions at the top of the second column.
- Select Deploy API.
- In the popup select dev as the Deployment stage.
- Click Deploy to deploy the API 

## 6 Enable CORS for S3
To be able to upload directly to an S3 bucket we also need to enable CORS for the bucket.
- Go to S3 in the AWS console.
- Click on the upload bucket (e.g. workshop-upload).
- Click Permissions from the bucket menu.
- Click CORS Configuration.
- Paste in the configuration from `4\cors.xml` and click Save.

## 8 Upload new website version
- Copy the config.js file containing your account specific settings, from the last part
`3/website/js/config.js` to `4/website/js/config.js1` 
- Navigate your consoles to the `4` folder and upload site:
```
 aws s3 sync .\website\ s3://wshp-static --region eu-west-1
```
- Open the website and sign in. Click on the plus button at the bottom of the
page to upload a movie file. You can use one of the example files from the first lesson. You will see a
progress bar while the upload takes place
- Go to the AWS console and have a look at the buckets. Did the file upload to the upload S3 bucket? Are there
new files in the transcoded S3 bucket? 
