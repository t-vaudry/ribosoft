#!/bin/bash

# exit on errors
set -e

DEPLOY_URL="$1"
DEPLOY_BRANCH="$2"

# decrypt necessary files
openssl aes-256-cbc -K $encrypted_201caca5971a_key -iv $encrypted_201caca5971a_iv -in .github/deploy/secrets.tar.enc -out .github/deploy/secrets.tar -d
tar xf .github/deploy/secrets.tar

# prepare ssh environment
cat .github/deploy/ssh_config >> /root/.ssh/config
eval "$(ssh-agent -s)"

# add deployment key
chmod 600 .github/deploy/deploy_key
ssh-add .github/deploy/deploy_key

# deploy to remote git repo
git remote add deploy $DEPLOY_URL
git push --force --quiet deploy $DEPLOY_BRANCH
