ver=1.2022.6.12
docker build -t "scholtz2/hasura-algorand-auth-web-hook:$ver-stable" -f HasuraAlgorandAuthWebHook/Dockerfile  ./
docker push "scholtz2/hasura-algorand-auth-web-hook:$ver-stable"
echo "Image: scholtz2/hasura-algorand-auth-web-hook:$ver-stable"
