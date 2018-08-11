# serverless-workshop
In this part, we are creating a transcoding logic to convert video between formats.

Here are some prerequisites:
- dotnet CLI
- aws CLI

## 1 Set your region
Login to AWS console and set region to Ireland (eu-west-1)
Make sure you use that region for all resources and services.

## 2 Create S3 buckets
- in the AWS console click on **S3**, and then click **Create Bucket**.
- enter a **bucket name**  (e.g. workshop-upload), and choose the region: Ireland.
- Click **Create** to save bucket.
- Repeat the process again to create another bucket (e.g. workshop-transcoded).
- **Note bucket names**, they will be used later.

## 3 Modify bucket policy
- In S3 click on the second bucket (*workshop-transcoded* bucket).
- Click on the **Permissions** tab
- Click **Bucket Policy**
- Enter the bucket policy (copy from *bucket-policy.json* file). Make sure to put actual bucket name instead of placeholder in policy.
- Click **Save**

## 4 Create IAM role for Lambda
The role will allow functions to work with S3 and the Elastic Transcoder.
- In the AWS console’s **Services** tab, click **IAM** under **Security, Identity & Compliance**, and then click **Roles**
from the left navigation menu
- Click **Create Role**
- In the **Trust** step, choose **AWS Service** and then **Lambda**, then click **Next: Permissions**
- In the Permissions step, search for and check the boxes for following items (Make sure the names you select match exactly):

   AWSLambdaExecute
   AmazonElasticTranscoderJobsSubmitter
   
- Click **Next: Review** to attach policies to the role.
• In the Review step, name the role lambda-s3-execution-role, and then click **Create role** to save.

## 5 Configure Elastic Transcoder
Elastic Transcoder does provide pipelines to perform video transcoding to different formats and bitrates.
- In the AWS console’s **Services** tab, click on **Elastic Transcoder** under **Media Services**, and then click **Create a New Pipeline**.
- Enter **name** and specify the **input bucket** (the first one created, workshop-upload).
- Leave the IAM role as it is. Elastic Transcoder creates a default IAM role automatically.
- Under **Configuration for Amazon S3 Bucket for Transcoded Files and Playlists** set resulting bucket (workshop-transcoded).
- Set the **Storage Class** to **Standard**.
- Even if not generating thumbnails, bucket and storage class still are required. Use the same parameters as for transcoding.
- Click **Create Pipeline** to save.
- **Note Pipeline ID**. It will be used later.

## 6 Create transcoding Lambda function
This is first lambda function creation, the implementation will be provided later.
- In the AWS console’s **Services** tab, click **Lambda** under **Compute**, and then click **Create function**.
- Click **Author from scratch**.
- On the basic information page, **Name** the function *transcode-video*.
- Set **Runtime** to **C# (.NET Core 2.1)**
- Under **Role**, select **Choose an existing role** and then **lambda-s3-execution-role**.
- Click **Create function**.
- Once the function is created, in the **Basic settings** section, set the **Timeout** to 0 minutes, 30 seconds.
- Set **Hander** to **VideoTranscoder::VideoTranscoder.Function::FunctionHandler**
- Scroll down to the **Environment variables**

  Add variable with Key **ELASTIC_TRANSCODER_PIPELINE_ID** and set its Value to pipeline ID you noted before (p. 5).

- At the top of the page, click **Save**.

## 7 Prepare and deploy Lambda
We will be using dotnet CLI to package and deploy Lambda functions.

First, you will need Lambda tooling installed:
```
dotnet new -i Amazon.Lambda.Templates
```

Now, to deploy a lambda, just run this command in Lambda's folder:
```
dotnet lambda deploy-function --fn transcode-video --region eu-west-1
``` 

## 8 Connect S3 to Lambda
Last step before trying pipeline. S3 will invoke Lambda and function will create transcode job in Elastic Transcoder

- While on Lambda page, scroll up to the **Designer** section
- Click on **S3** in the **Add triggers** list on the left
- Scroll down to the **Configure triggers** section
- Select the upload bucket (e.g. workshop-upload).
- In the event type dropdown, select **Object Created (All)**.
- Press **Add**
- Click on **Save** button.

## 9 Testing
To test the function in AWS, upload a video to the upload bucket.
In the workshop root directory there’s a sample-videos.zip containing videos you can use.

- go to **S3**, navigate to the *workshop-upload* bucket, and then select **Upload**
- Click **Add Files**, select a video file (an .avi, .mp4, or .mov), and click **Upload**. The file you selected should
appear in the bucket.
- Navigate to the *workshop-transcoded* bucket, and after a short period of time files will appear. They would be put in a folder
rather than in the root of the bucket.
