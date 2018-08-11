# serverless-workshop
In this lesson, we’ll connect our video transcoding pipeline in AWS with our website so that website users can
view transcoded videos.
We’ll do this using an online database service called Firebase. Firebase is a no-SQL database that has rich
JavaScript support and can stream data updates directly to user’s connected devices using web-sockets.



## 1 Create Firebase DB
First, you need to create a Firebase account.
- Visit https://firebase.google.com/ to create a free account.
- Click the Get Started button.
- Register with your Google Account.
- You will be taken to the console. In the console click Add Project. 
- Give your project a name like, “serverless-workshop”, and click Create Project 
- Your project will be created, which comes with a database. Let’s check it out.
- Click Develop, then Database in the left navigation menu. Then click the CREATE database button in the Realtime Database tile 
- Choose Locked mode
- You’ll see you have an empty database. That’s OK, we’ll add some data later. For now, take note of the database URL. You will need it. 
- Click on the Rules tab.
- Set “.read” to true.  Set ".write" to "auth != null" (Quotes are to be included)
- Click the PUBLISH button to save 

## 2 Modify website to access database

- Copy the config.js file containing your account specific settings
- Go the Firebase console and click on Project Overview in the upper left hand corner.
- Click on Add Firebase to your web app circle and copy the config object. 
- Paste the config object to `5/website/js/video-controller.js` where it says: /*PASTE CONFIG HERE*/ in connect your website to Firebase. 
- Open `5` in your terminal/console and upload site:
```
aws s3 sync .\website\ s3://workshop-static --region eu-west-1
```
- Reload your app. You should see a blue spinner in the center of the page. This will continue spinning until some videos are found in Firebase, which we’ll add next.

## 3 Test with some sample data
To validate that our web site is connected to Firebase, we’ll load some sample data into the Firebase database.
This data points to some videos we’ve already transcoded for you.
- To show off the push-based update features of firebase, make sure the site is open on your screen.
- In another browser window, open your Firebase project’s database and go to the Data tab, just like you did in Step 1.
- Press the Hamburger button in the top right-hand corner of the database section From the menu select Import JSON.
- Upload the json file from `5/data/firebase-sample-data.json`
- Your web site will be automatically updated, as Firebase pushed the new data directly to your web browser via web sockets. 

## 4 Modify video transcode lambda function for Firebase 
 Before we proceed to modify Lambda functions we need to create service accounts in Firebase.
- In Firebase click on the Gear icon next to Project Overview in the left navigation menu, and click Users and permissions.
- Select Service Accounts tab.
- Click Manage all service accounts link
- Click the Create Service Account button.
- Set a service account name like “workshop”.
- From the Role dropdown, select Project and then Editor.
- Click “Furnish a new private key” and make sure that JSON is selected.
- Leave everything else as is and click SAVE
- Click CLOSE on the popup that appears
- Copy the JSON file that was downloaded to `5/transcode-video-firebase-enabled` and make a note of the filename 

Now we’re going to modify the existing video-transcode Lambda function, to have it push a new entry into
Firebase with transcoding: set to true. With this in place, the user interface will show a placeholder of a video
showing an animation while the video transcodes.

In Part 4 we configured file uploads, and this upload system created a unique key for each file that was
uploaded (this key was used in the path when the file was stored in S3). We’ll have our Lambda function use
this key as the unique key for the video in Firebase 


ZIP up your lambda function. For OS X / Linux Users:
- In the terminal / command-prompt, change to the directory of the function:
`cd 5/transcode-video-firebase-enabled`
- Install npm packages by typing:
`npm install`
- Now create create a ZIP file of the function, by typing:
`npm run predeploy`

For Windows

- You will need to zip up all the files in the 5/video-transcoder-firebase-enabled folder via
the Windows Explorer GUI, or using a utility such as 7zip. (Note: don’t zip the video-transcoderfirebase-enabled folder. Zip up the files inside of it) 

- In the AWS console, go to Lambda.
- Select the transcode-video function
- Change Runtime to NodeJS 6.10
- Change Handler to index.handler
- Choose Upload a .ZIP file from Code entry type and click Upload.
- Select the ZIP file of the Lambda function you just created.
- Create environment variables:

   ELASTIC_TRANSCODER_REGION  eu-west-1
   
   ELASTIC_TRANSCODER_PIPELINE_ID 
   
   SERVICE_ACCOUNT: The name of the JSON Service Account file you created earlier in this step.
   
   DATABASE_URL: The URL from the Database tab in the Firebase.
- Click the Save button at the top of the page to upload the function and save your environment variables. 

Test that your function works by opening the web-site and uploading a video. Within a few seconds of the upload completing, you should see a new entry appear in the video list, showing the transcoding animation. This animation will remain forever, because we haven’t yet connected anything to update firebase once the transcoding has completed 


## 5 Create new Lambda function: PUSH-TRANSCODED-URL-TO-FIREBASE 
Now we’re going to complete the final piece of the system: We’re going to add a new lambda function, that
will trigger every time a newly transcoded video arrives in the second, transcoded S3 bucket. This lambda
function will write the public URL of the video to Firebase (so that the browser can play the video). It will also
set transcoding: false, indicating that transcoding has completed 
- Open a terminal / command-prompt and navigate to the following folder:
`5/push-transcoded-url-to-firebase`

- Install npm packages by typing:
`npm install`
- Copy the Firebase service account JSON file
- Copy the Firebase service account file you created in the previous step to `/5/push-transcoded-url-to-firebase`. The JSON file must be included with the Lambda function for it to work.


Now ZIP up your lambda function. For OS X / Linux Users
- create a ZIP file of the function, by typing:
`npm run predeploy`

For Windows:

- You will need to zip up all the files in the `5/push-transcoded-url-to-firebase` folder via
the Windows Explorer GUI, or using a utility such as 7zip. (Note: don’t zip the push-transcoded-urlto-firebase folder. Zip up the files inside of it). 

In the AWS console, go to Lambda and create a function as before, with the following settings:
- Name: push-transcoded-url-to-firebase
- Runtime: Node.js 6.10
- Role: lambda-s3-execution-role
- Function package: The .zip file you just created.
- Timeout: 30 seconds
- Environment variables:

   SERVICE_ACCOUNT: the name of the JSON Service Account file you created earlier
   
   DATABASE_URL: the database URL from Firebase, just as you did for the last function
   
   S3: the URL of your second (transcoded) bucket, e.g.
https://workshop-transcoded.s3.amazonaws.com/.
   **NOTE** the format of the endpoint, the bucket name is just after `http`
   
   BUCKET_REGION: eu-west-1

- Don’t forget to click the Save button at the top of the page


## 6 Modify transcoded video bucket to trigger new lambda function
Now we need to configure S3 to invoke the new push-transcoded-url-to-firebase lambda function when a
newly transcoded video arrives in the destination bucket:
- In the Lambda console for the push-transcoded-url-to-firebase function you just created, scroll to the Designer section
- Click on S3 in the Add triggers list on the left 
- Scroll down to the Configure triggers section
- Select the second bucket (e.g. workshop-transcoded).
- In the event type dropdown, select Object Created (All).
- Enter a suffix of .mp4

We do this to ensure that the lambda function is only called when new videos arrived. The elastic
transcoder may drop other assets in the bucket (such as thumbnails, or JSON files) which should not
trigger the lambda function 

- Press Add, then click save up the top and AWS will link your S3 bucket and Lambda function 

## 7 Test system END-TO-END
- Open the website in your browser.
- Upload a video file. The progress bar will show as the video uploads.
- Once upload completes, a tile will appear in the user interface representing the video. It will contain an animation, indicating that the video is being transcoded.
- Once transcoding is complete, the transcoding animation will be replaced by the video.
- Click on the video to watch it play. You’ll notice that this is running in a web-friendly, lower quality
480p format 


















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
