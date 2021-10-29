for file in ./*
do
  kubectl apply -f "$file"
done
