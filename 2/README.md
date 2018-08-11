# serverless-workshop
In this part, we’ll	create a	website	for	our	video	hosting	platform and integrate the	website	with Auth0.

##  1 Create S3 bucket and host site
- in the AWS console click on **S3**, and then click **Create Bucket**.
- enter a **bucket name**  (e.g. workshop-static), and choose the region: Ireland.
- Click **Create** to save bucket.
- **Note bucket name**, it will be used later.
- Choose the **Permissions** tab, then choose **Bucket Policy**.
- Enter the following policy document into the bucket policy editor replacing YOUR_BUCKET_NAME with the name of the bucket:
```
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "arn:aws:s3:::YOUR_BUCKET_NAME/*"
        }
    ]
}
```
   This policy will allow public reads from the bucket.
- Click **Save**
- Choose the **Properties** tab, then **Static website hosting** card.
- Select **Use this bucket to host a website** and enter index.html for the Index document. Leave the other fields blank.
- **Note** the **Endpoint URL** at the top of the dialog. This will be the site address.
- Click **Save**

## 2 Create AUTH0 Account
You’ll	need	to	create	a	free	auth0	account.	Visit	https://auth0.com and	follow	the	sign	up	steps.
- You’ll	be	asked	to	enter	a **tenant domain**.	Enter	a	name	that	is	unique	to	you.
- Enter	“**EU**” as	your **region**.
- Click	**Next** and	fill	out	the	information	on	the	next	page.
- Click	**Create	Account**.
- Go	to	**Clients** in	the	left	navigation	menu,	and	click	on	the	**Default	App**.
- Go	to	**Connections** in	the	**Default	App** menu,	and	make	sure	that	only	**Username-PasswordAuthentication** is	enabled
- Go	to	**Settings** in	the	**Default	App** menu.
- Scroll	down	until	you	find	the	textbox	called	**Allowed	Callback	URLs**.

   Enter the	endpoint address of your application, noted in p.1
- Enter	the	same	value	into	the	following	fields:	**Allowed	Web Origins**,	**Allowed	Origins	(CORS)**
- Scroll	down	to	the	bottom,	and	click	the	**Show	Advanced	Settings** link.
- Under	**Advanced	Settings**,	choose	the	**OAuth** menu,	and	ensure **JsonWebToken	Signature	Algorithm**
is	set	to **RS256**.
• In	the	same	section,	make	sure the	**OIDC	Conformant** option is	green
- Scroll	down	and	click	the	**Save	Changes** button.
• We	now	need	to	retrieve	some	values	from	Auth0	that	will	be	needed	throughout	this	workshop.
Scroll	up	to	the	top	of the	same	**Settings** page,	and	find	the	**Domain**,	**Client	ID** and	**Client	Secret**.

   **Note** their values,	we will need them throughout the workshop.

## 3 Setup website
- Edit	the	following	file:
```
2/website/js/config.js
```
- Enter	your	auth0	**domain** and	**client	ID** from previous step and save.
- Open	a	terminal	/	command-prompt	and	navigate	to	the	following	folder:
```
2
```
- Run (replace placeholder with bucket name from step 1):
```
aws s3 sync .\website\ s3://YOUR_SITE_BUCKET_NAME --region eu-west-1
```

## 4 Check results

- Open a browser and try the site!
- Click	**Sign	In** button	in	the	top-right to	launch	the	authentication	popup:

   This	popup	is	rendered entirely	by	Auth0. You will need to sign up.
- You’ll	be	automatically	signed	in	after	your	account	is	created.
