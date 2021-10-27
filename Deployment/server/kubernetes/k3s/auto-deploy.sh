kubectl apply -f ./logging/namespace.yaml
kubectl apply -f ./volume/volume.yaml

kubectl apply -f ./configmap/dbmanagerconfig.yaml




kubectl apply -f ./ingress/ingress.yaml



kubectl apply -f ./deployment/dbmanager.yaml
kubectl apply -f ./deployment/conductor.yaml
kubectl apply -f ./deployment/signalling.yaml
kubectl apply -f ./deployment/slavemanager.yaml
kubectl apply -f ./deployment/landing-page.yaml
kubectl apply -f ./deployment/service-page.yaml
kubectl apply -f ./deployment/admin-page.yaml

kubectl apply -f ./logging/elasticsearch.yaml
kubectl apply -f ./logging/elastic_svc.yaml
kubectl apply -f ./logging/fluentd.yaml
kubectl apply -f ./logging/ingress.yaml
kubectl apply -f ./logging/kibana.yaml

