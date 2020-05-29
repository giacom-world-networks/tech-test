# Giacom Tech Test

## Background
Giacom aka ‘Cloud Market’ is a B2B e-commerce platform which allows IT companies (resellers) to buy cloud services indirectly from major vendors (Microsoft, Symantec, Webroot etc) in high volumes at low cost. IT companies then resell the purchased services on to their customers, making a small margin. Behind Cloud Market are several microservices, one of which is an Order API much like the one we are going to work on for this test.

## Concepts
* Reseller = A customer of Giacom
* Customer = A customer of a Reseller
* Order = An order placed by a Reseller for a specific Customer
* Order Item = An item for a particular service and product which belongs to an Order
* Order Status = The current state of a specific order
* Product = An end-offering which can be purchased e.g. '100GB Mailbox'
* Service = The category the Product belongs to e.g. 'Email'
* Profit = The difference between Cost and Price

## Time
You should allocate approx. 2 hours to complete the tech test.

## Pre-Reqs
* Visual Studio (or compatible IDE for working with .net Core)
* Git
* Docker (running Linux containers)
* Heidi / MySQL Workbench (database client)
* Postman (can also use other API client / browser)

## Setup
1. Fork this repository and clone it locally
2. Using a terminal, cd to the local repository and run 'docker-compose up db', which will start and seed the database
3. Check connectivity to the local docker database is working by opening your database client and connecting to:
   Hostname: localhost
   Username: order-service
   Password: nmCsdkhj20n@Sa
4. Open the solution file in /src
   
## Tasks
1. Add a new API endpoint which returns orders with a specified status e.g. 'Failed'
2. Add a new API endpoint which allows an order status to be updated to a new status e.g. 'InProgress'
3. Add a new API endpoint which allows an order to be created. This should include validation of any parameters.
4. Add a new API endpoint which calculates profit by month for all completed orders

Finally, once code-complete, close your IDE, run 'docker-compose down --volumes' to stop and remove the database container. Now run 'docker-compose up'. This will run the local database and also build the microservice in Release mode. Test the API is working correctly via this method (as this is the one Giacom will run to test the submission).

## Submission
Email development@giacom.com (TBC - Need a new mailbox?) a link to the forked repository or a zip file containing the repository. If applicable, add notes in the email to explain why you have chosen a particular approach.

## Help
If you happen to run with any issues when running the Docker container, try deselecting Hyper-V Services in "Windows Features" (Search for Windows Features in Start Menu), selecting again, and then restarting your computer.

If you experience any further issues getting set up with the tech test please email development@giacom.com (TBC - Need a new mailbox?).

Copyright (c) 2020, Giacom World Networks Ltd.
