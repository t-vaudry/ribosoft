*** Settings ***
Documentation           Install the docker image and start the ribosoft service
Library                 DockerComposeLibrary


*** Keywords ***
Start Ribosoft
    Docker Compose Up  

Stop Ribosoft
    Docker Compose Down