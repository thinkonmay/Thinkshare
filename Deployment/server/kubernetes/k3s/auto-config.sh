

curl -sfL https://get.k3s.io | sh -

k3s kubectl get node
kubectl apply -f https://github.com/jetstack/cert-manager/releases/download/v1.5.4/cert-manager.yaml