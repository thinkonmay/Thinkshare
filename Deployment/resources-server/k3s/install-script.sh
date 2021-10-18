
kubectl apply -f https://github.com/jetstack/cert-manager/releases/download/v1.5.4/cert-manager.yaml
kubectl apply -f ./volume/volume.yaml
kubectl apply -f ./volume/volume-claim.yaml
kubectl apply -f ./ingress/ingress.yaml