# Thinkshare
A platform allow user to hire computers from the others, developed by Thinkmay
## Mission:
Our ultimate goal is to change how technology serve human and shape our knowledge about the world

## Quick Links:
- Website: https://www.thinkmay.net
* Slack (chat channel): https://join.slack.com/t/thinkmayworkspace/shared_invite/zt-ywglslgj-fQb4Po4JagVaHbZ8wwiqpg

## Document
* Architecture document (miro): https://miro.com/app/board/o9J_lTKComc=/?invite_link_id=202014558866
* Detailed document (notion): https://thinkonmay.notion.site/5a4909c660374a4ca0286d766bf3b9f1?v=bd0da1b672c14c6fbe2f2ad4d29b99b7


# Build and Debug Instructions 
## Ubuntu  
The following steps will configure your Ubuntu machine for building and testing personal cloud computing server.
* Install dotnet 5.0 runtime and SDK.
    * Link: https://docs.microsoft.com/en-gb/dotnet/core/install/linux-ubuntu 
* Install the latest LTS Node:
	* Link: https://nodejs.org/
* Install docker 
    * Link: https://docs.docker.com/engine/install/ubuntu/
* Clone the git repository: 
    * `git clone https://github.com/pigeatgarlic/personal-cloud-computing`

## Testing 
Thinkshare repo contain 2 test for token generator and websocket connection
* Token generator test:
    * `cd Test/Token`
    * `dotnet test`

# Deployment
Thinkshare server is deployed using terraform on AWS elastic kubernetes service

* Terraform configuration:
    * `Deployment/terraform/prod/eks`
* Kubernetes manifest:
    * `Deployment/kubernetes/eks`

More information about terraform and kubernetes can be found at:
* Terraform: https://www.terraform.io/
* Kubernetes: https://kubernetes.io/



## GitHub Actions
Thinkshare use github action as  the main method for continous intergrations and continous deployment (CI/CD) as well as docker image versioning

Whenever a push or a pull request is apply on mster branch, github action will automatically build and push new docker image with the tag base on current date 
* For example: pigeatgarlic/authenticator:2021-12-03 


The definitions for the build processes are located in `/.github/workflows/` folder.

# Database
Thinkshare database use postgresql as user database and redis to store state of the system
* More information about postgresql can be found at https://www.postgresql.org/
* More information about redis can be found at https://redis.io/


## Infrastructure
Thinkshare database is hosted by AWS relational database service (RDS) and managed by postgreadmin
* To access database manager, goto https://database.thinkmay.net/ with admin credential

## Migrations 
* Our migrations method is code first using ASP.NET entity framework, migrations result stored in
    * `Host/Conductor/Migrations`
* To migrate and update database goto
    * `cd Host/Conductor`
    * `dotnet ef migrations add {migration-name} --context GlobalDbContext`
    * `dotnet ef database update --context GlobalDbContext`
One should be vary carefull when perform database migrations, 

# Logging
We use EKF (ElasticSearch-Kibana-Fluentd) to collect logging of kuebernetes cluster as well as log from docker image.

* To access kubernetes log, goto https://logging.thinkmay.net/.


# Contact 
If there is any problem related to personal cloud computing, please contact at email:
* contact@thinkmay.net
