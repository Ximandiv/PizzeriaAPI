# How to Run

### Important

Make sure to fill in your values showcased in .env-template into a new .env

### General

This REST API Web Service Application uses docker and docker-compose for development and production purposes.
Thus, you need to create a .env file based on the .env-template file you will find inside the project.

Be sure the DB connection string user and password are the same as the MY_SECRET_USER and MY_SECRET_PASSWORD respectively.

### Make sure you open up the terminal in the location where the Dockerfile is located at
Once you fill it in... Make sure you have Docker installed to run the following commands in the terminal:
- docker build -t pizzeriaapi-server:latest .
- docker-compose up

To access the web page, make sure that you can see the .NET console log saying that it's listening on
port 8080. Finally, enter the following link: http://localhost:5283/swagger/index.html

The reason we use port 5283 is because the container uses 8080 but the host machine makes a proxy for both
the web app and database. So they are accessible at 5283 and 3307 respectively from the host machine itself.

If you want to turn off the application, you can stop it in Docker Desktop, CTRL C in the terminal or write this command in the compose.yaml location with the terminal
- docker-compose down