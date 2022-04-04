ver=1.0.0
docker build -t "scholtz2/hasura-algorand-auth-web-hook:$ver-stable" -f HasuraAlgorandAuthWebHook/Dockerfile  ./
#docker push "scholtz2/hasura-algorand-auth-web-hook:$ver-stable"
echo "Image: scholtz2/hasura-algorand-auth-web-hook:$ver-stable"
