#!/bin/bash

# exit on errors
set -e

DEPLOY_URL="$1"
DEPLOY_BRANCH="$2"

# decrypt necessary files
openssl aes-256-cbc -K $encrypted_201caca5971a_key -iv $encrypted_201caca5971a_iv -in .travis/secrets.tar.enc -out .travis/secrets.tar -d
tar xf .travis/secrets.tar

# prepare ssh environment
cat .travis/ssh_config >> ~/.ssh/config
eval "$(ssh-agent -s)"

# add deployment key
chmod 600 .travis/deploy_key
ssh-add .travis/deploy_key

# deploy to remote git repo
git remote add deploy $DEPLOY_URL
git push --force --quiet deploy $DEPLOY_BRANCH
