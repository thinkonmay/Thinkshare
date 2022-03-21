# Thinkshare
A platform allow user to hire computers from the others, developed by Thinkmay

## Quick Links:
* Slack (chat channel): https://join.slack.com/t/thinkmayworkspace/shared_invite/zt-ywglslgj-fQb4Po4JagVaHbZ8wwiqpg

## Document
* Architecture document (miro): https://miro.com/app/board/o9J_lTKComc=/?invite_link_id=202014558866
* Document (Notion): https://thinkonmay.notion.site/Thinkshare-17ba71410040428590d618e51da3c30e
* Timeline (Notion): https://thinkonmay.notion.site/thinkonmay/Features-timeline-4eaf284ba59e4355a95fa6200b8288f1

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


# Contact 
If there is any problem related to personal cloud computing, please contact at email:
* contact@thinkmay.net
