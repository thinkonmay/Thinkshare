for file in ./*/*.yaml
do
  printf "deploy manifest file $file \n"
  kubectl apply -f "$file"
done