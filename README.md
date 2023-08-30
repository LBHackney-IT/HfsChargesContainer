# HfsChargesContainer
EC2 container for Housing Finance System "Charges Import" Nightly Process.

The code of this nightly process was moved from the [Housing Finance Interim API](<https://github.com/LBHackney-IT/housing-finance-interim-api/> "Code repository link") application where it was previously running as a cron-triggered AWS Step Function _(each step AWS Lambda hosted)_. The code had to be moved due to running into 15 minute AWS Lambda processing timeout limits. This way-of-hosting the application change solves the problem by having the code run as an AWS Fargate EC2 instance (no execution time limit), which gets triggered via the same cron expression.

The code was moved as is with minor tweaks to the Use Case return types & logging, which were AWS Lambda and AWS Step Function specific. This moved part of the interim API had absolutely no tests to it, therefore there were none to be moved. This is technical debt, which will be addressed as part of technical debt backlog.

# Pipeline and Infrastructure
## AWS Infrastructure
The infrastructure for this application is defined within the [MTFH Finance Infrastructure](<https://github.com/LBHackney-IT/mtfh-finance-infrastructure> "Code respository link") repository, where its code specifically was brought it by [this PR](https://github.com/LBHackney-IT/mtfh-finance-infrastructure/pull/19).


## CI/CD pipeline
The deployment bit of this repository's pipeline merely builds & deploys the application's docker image to the relevant AWS ECR repository. Each image is deployed as 'latest', making the image before lose its 'latest' tag.

The EC2 Task definition within the AWS is configured to point to the 'latest' image within an ECR. Therefore upon the Task _(based on the Task definition)_ getting created due to the cron trigger, the Task should be always pointing to the latest ECR image.

The `main` branch represents the `development` environment, while the `release` branch is for `staging` and `production` environments. The release to `production` environment is behind the manual 'permit' button that appears after deployment to `staging` is complete.

# Running
This application is supposed to run as docker container.
1. Create a copy of `.env.sample` and then populated with the required values.
    ``` sh
    cp .env.sample .env
    ```
2. If you intend to run the application in non-local mode _(you set the ENVIRONMENT to anything other than 'local')_, you will need to add in the AWS programmatic access credentials into your `.env` file for the application to be able to access the AWS SSM Parameter Store and AWS SNS Topic for sending email alerts.
2. Build and run the application docker container using a Makefile
    ``` sh
    make run
    ```

# Tests
At the time of writing, the tests image of this application does not depend on any external dependencies like another database image, but this may change going forward.

To run the application tests, run:
``` sh
make test
```

Until a better clean up process is introduced, you may need to occassionally clean up your docker images on your host machine manually.

## Dependencies
The application depends on:
* Docker - the Application is intended to be run as a docker container.
* Housing Interim API SQL database - all the operations are done against this database.
* AWS SNS Topic - _([see infrastructure](<https://github.com/LBHackney-IT/mtfh-finance-infrastructure> "Code respository link"))_ for sending out emails notifications on application failure. _(This is disabled by default for the local run)_
* AWS SSM Parameter Store - The application is configured to pick up all of the environment variables _(except the `ENVIRONMENT` and `SNS_TOPIC_ARN`)_ from the parameter store. This simplifies the new financial years preparation as well as any database credentials changes as this allows avoiding the redeployment of the application's infrastructure. 

# Nightly run
This process is only 1 part of the entire nightly run. The rest of the process parts are deployed as part of the [Housing Finance Interim API](<https://github.com/LBHackney-IT/housing-finance-interim-api/> "Code repository link") application. See more information about the nightly run within these diagrams [link](https://drive.google.com/drive/u/0/folders/17ht3pLvOmC3WTMxSagid_MnubMTsSiI1).
