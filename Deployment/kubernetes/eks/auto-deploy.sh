# kubectl apply -f namespace.yaml

# kubectl apply -f ../argocd/argocd.yaml -n argocd
# kubectl apply -f ../logging/ -n kube-logging 

# for file in ./*/*.yaml
# do
#   printf "deploy manifest file $file \n"
#   kubectl apply -f "$file"
# done

# volume mount point
kubectl apply -k "github.com/kubernetes-sigs/aws-efs-csi-driver/deploy/kubernetes/overlays/stable/?ref=master"

#https://github.com/marcel-dempers/docker-development-youtube-series/tree/master/storage/redis/kubernetes#readme

kubectl apply -f ./redis/redis-config.yaml
kubectl apply -f ./redis/redis-deploy.yaml

kubectl -n backend get pods

kubectl -n backend logs redis-0
kubectl -n backend logs redis-1
kubectl -n backend logs redis-2



# kubectl -n backend exec -it redis-0 sh
# redis-cli 
# info replication

kubectl apply -n backend -f ./redis/redis-sentinel.yaml

kubectl -n backend logs sentinel-0