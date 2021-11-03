kubectl apply -f namespace.yaml

kubectl apply -f ../argocd/argocd.yaml -n argocd
kubectl apply -f ../logging/ -n kube-logging 

for file in ./*/*.yaml
do
  printf "deploy manifest file $file \n"
  kubectl apply -f "$file"
done