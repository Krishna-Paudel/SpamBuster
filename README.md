**SpamBuster**

**INTRODUCTION**

SpamBuster is a lightweight, reliable, and easy-to-use security tool that scans your emails to identify spam. This tool is powered by a machine learning model behind the scenes which is hosted in the Google Cloud. This tool automatically scans the incoming emails as well as provides you with an option to manually scan the existing emails or folders. This tool comes in the form of an Outlook plug-in that can be used right from your Outlook Explorer window.

**DESIGN**

There are three main components in the SpamBuster Solution: Outlook Add-In, ML Model, and Cloud Function

1. **SpamBuster Outlook Add-In**

This is an Outlook plugin also known as Outlook Add-In built with C# on a .NET Framework. This application is the GUI interface for our Spam Ham Classifier project.

Features of the Add-In:

1. Automatically scans the newly received emails in any of the Outlook folders.
1. Provides On-Demand scans of the Outlook folders for potential spam.
1. Moves the Spam to the Junk Folder.
1. Interacts with Google Cloud Function through an API for prediction.
1. Asynchronous programming to make the Outlook program interactive during the operation.
1. Logs all the scanning activity into the folder: %LocalAppData%\SpamBusterLogs\

The SpamBuster Outlook Add-In was developed using Visual Studio Office Developer Tools on .NET Framework with C#. This solution includes

1. **ThisAddIn.cs**

This class is responsible for starting the Add-In activities when Outlook starts. It sets the log4net configuration and initiates the logger. It registers a listener event in case a new email is received when the Outlook application is running. When a new mail arrives, the listener calls a method that captures the mail and sends it for further processing to predict the email category as spam or ham. If predicted as spam, it will move the email to the Junk Folder of the user’s email account.

2. **SpamBusterRibbon.cs**

This class is responsible for generating the SpamBuster UI which is a button in the Outlook Explorer Ribbon. At any given point, when this button is clicked, it will ask the user to pick a folder to scan. This is typically useful when you want to scan your email On-Demand or when you are installing the add-in for the first time and would like to scan the existing emails.

When an email is being scanned, this class has a method that collects the properties of the email like headers, email body, email subject, etc. to the gerPrediction() method of SpamBusterModel.cs class for further processing. Once the email category is predicted, the email will be appropriately handled i.e., it will be disposed of to the Junk folder if it is found to be spam.

3. **ProgressBar.cs**

This class is responsible for tracking the progress when a scan request is submitted. It shows the total number of emails it is scanning and the current progress. One can click “Cancel” to stop the scanning in the mid of the operation. It will also show Warning messages when you try to submit more than one operation while one is in progress.

4. **SpamBusterModel.cs**

This class includes the most important methods of the whole solution. It extracts useful authentication headers like SPF, DKIM, and DMARC flags that are used as raw features for prediction. Once all the features are collected, it sends this information to a SpamBuster Cloud Function through an API call that uses HTTPS POST Method. The API call submits the input to the function, which processes and cleans it before submitting it to the ML model for prediction. Once predicted, the result is sent back as a response to the HTTPS POST request.

The communication between the Outlook Add-In and the API in Google Cloud Function is encrypted using TLS 1.2, hence all the data being transferred are encrypted and safe.

5. **log4net config**

Apache Log4Net is a logging framework that can be used to capture logs. This is useful for debugging, logging warnings and errors, and any information during the program execution. It is very easy to use and flexible to configure various useful options with a simple change in its configuration. In SpamBuster, the log4Net collects logs of various natures like INFO, WARN, ERROR, etc. along with a timestamp and relevant message.

The log files will be stored at %LOCALAPPDATA%\SpamBusterLogs\

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/29698c1d-a713-4743-afd6-df84da5faa73)


**How to use the Outlook Add-In?**

***DISCLAIMER**: Before you proceed with running the Outlook Add-In, please understand that if an email is predicted as SPAM, it will be moved from its original location to the Junk/Spam folder. The ML model is not 100% accurate as it is trained on the sampled dataset, so please be cautious while running it. We recommend running it in the folder of emails that are not important. Or you can also move back emails from the Spam/Junk folder if it's miscategorized. However, please be aware that most mailboxes retain spam for 30 days only and you may lose the data.*

We have built a Click Once windows application that will install the plugin into your Outlook application.

- Extract the compressed file SpamBuster-Outlook-Addin.zip to a local path.
- Double-click on the setup.exe file and Click Install
- Restart the Outlook application.

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/d74b06a2-806e-4926-ae0d-d6cca68f02df)


- Once installed, locate the Buster menu in the Outlook Explorer Ribbon. Click on Scan All.

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/798e0504-f567-49d6-ae15-f7a81e0cd02d)


- Pick the folder you want to scan the emails from.

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/7d8e87d3-6dd4-471b-96fa-f0370b8265ca)


- Monitor the progress or cancel it during the mid of the operation.

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/5c8cf160-cf14-43ed-89f7-5db8a0ee2eb0)![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/0c65b8cc-3426-4f0f-bb82-964f3cd36a4a)


- You will notice that the mail identified as spam will be moved to the Junk or Spam folder.

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/bf270bdd-9dec-4648-9752-b22e7a92b669)


2. **SpamBuster Cloud Function**

The SpamBuster ML Function is a flask application deployed to the Google Cloud Function with a Python Environment as an API endpoint. This function accepts a HTTPS POST/GET Request through the API with the raw input features for the prediction of Ham or Spam emails. The input values are then parsed and preprocessed using the tokenizer and encoders pickled while training the model. Once the input values are encoded and tokenized, they are passed to the ML model which is restored using its pickle file. The model is then used to predict the email category.

The reason behind selecting GCP over Azure and AWS was the flexibility of scaling up the SKU of the function while using the free trial version. We have used 4 GB of Disk size and 2 vCPU to run the function on Google Cloud that serves as an API endpoint.

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/ff9a1e7f-6a49-4f4c-a640-d1c31bc12ba1)![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/6f5b8111-c909-4636-bf8e-f5645b96c756)



In short, the cloud function does the following in order:

1. Triggers when an HTTPS Request is received.
1. Parses the input sent in the JSON body
1. Cleans and pre-processes the input strings
1. Tokenizes the texts and encodes them using tokenizer and encoder pickle files.

**Performance Metrics**

We have captured Fiddler trace to determine the round trip time of the API Request and Response. The average round trip time taken for the request-response is approximately 1 second.

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/d7a0406d-c2d8-4d84-b3c4-fbe04d46e262)


Some of the other performance metrics on the Cloud Function (API) is attached below:

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/87b5dea2-2690-4c55-99ac-e35324856906)

![image](https://github.com/Krishna-Paudel/SpamBuster/assets/52009770/778593b4-d6a5-4459-9641-84a0f6486573)


From the above snapshots, we can see that the performance counters like Memory, CPU utilization is pretty low when we were load testing it by sending the 200+ request in sequence with no delay. The response is served within less than a second or in few milliseconds making the API solution pretty feasible with the deployed ML model.

3. **SpamBuster ML Model**
<<To be updated..>>
