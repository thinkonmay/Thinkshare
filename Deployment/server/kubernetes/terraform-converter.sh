for file in ./eks/*/*.yaml
do
  filename=$(basename $file)
  printf "converting file $file \n"
  i=${filename%.*}
  cat "$file" | ./k2tf > ../terraform/aws/provision/"$i".tf
done

for file in ./gke/*/*.yaml
do
  filename=$(basename $file)
  printf "converting file $file \n"
  i=${filename%.*}
  cat "$file" | ./k2tf > ../terraform/gcloud/provision/"$i".tf
done