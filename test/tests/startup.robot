*** Settings ***
Documentation           Test startup functionality
Library                 SeleniumLibrary
Resource                ../resources/install.resource
Test Setup              Start Ribosoft
Test Teardown           Stop Ribosoft


*** Variables ***
${URL}        http://localhost:5001
${BROWSER}    headlesschrome
${TITLE}      Home Page - Ribosoft

*** Test cases ***
Startup
    Sleep    30s
    Open Browser    ${URL}    ${BROWSER}
    Title Should Be   ${TITLE}
    Close Browser