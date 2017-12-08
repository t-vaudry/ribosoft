#!/bin/bash

# exit on errors
set -e

DEPLOY_URL="$1"

# prepare ssh environment
echo -e "$SSH_CONFIG" >> ~/.ssh/config
eval "$(ssh-agent -s)"

# decrypt deployment key
openssl aes-256-cbc -K $encrypted_201caca5971a_key -iv $encrypted_201caca5971a_iv -in .travis/deploy_key.enc -out .travis/deploy_key -d
chmod 600 .travis/deploy_key
ssh-add .travis/deploy_key

# deploy to remote git repo
git remote add deploy $DEPLOY_URL
git push --force --quiet deploy develop
