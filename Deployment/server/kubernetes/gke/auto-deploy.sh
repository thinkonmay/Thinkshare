kubectl apply -f ./ingress/ingress.yaml
kubectl apply -f ./deployment/conductor.yaml
kubectl apply -f ./deployment/signalling.yaml
kubectl apply -f ./deployment/slavemanager.yaml
kubectl apply -f ./deployment/landing-page.yaml
kubectl apply -f ./deployment/service-page.yaml
kubectl apply -f ./deployment/admin-page.yaml