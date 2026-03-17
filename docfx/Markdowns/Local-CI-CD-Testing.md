## MESS CI/CD Local Testing
When creating new workflows we have found that it is easier to use a CLI tool called [act](https://github.com/nektos/act) to test the workflow locally before testing in production.

### Setup
To setup act, we recommend following the installation guide from the official documentation here: https://nektosact.com/installation/index.html. They offer a much more comprehensive view on how to setup the tool.

***

In terms of workflows, the _build-and-test_ flow will work with no other setup aside from running Docker as an administrator. The _E2E_ test flow will require that a **.secrets** file be created that contains a variable called
```txt
TEST_DB_PASSWORD=ENTERAPASSWORDHERE
```
Note: This file should **NOT** be checked into version management. Although it is used for testing, we think it is better to have the explicit file creation for each development setup.

### Test times
_E2E _Testing locally takes anywhere from 5-10 minutes to run, depending on the internet connection.
_Build&UnitTest_ locally takes <5 minutes on average to run.

### Configuration
For the initial **act** setup we choose to use the **Medium Docker** config, but you may be able to interchange with the **Micro **and/or the **Enterprise**


