# JJMasterData.MongoDB

# Intro
Plugin used to add support for the MongoDB database.
At the moment only the `IDataDictionaryRepository` interface (used to store your database metadata) is implemented.

# Motivation

The metadata in JJMasterData is represented by JSON objects. We believe that it 
makes more sense to store metadata in JSON format 
in a NoSQL database than in a relational database. <br>
In the future it is also planned to implement the forms data storage (`IEntityRepository`).

## Configuration
Add the following method to `MasterDataServiceBuilder`:
```cs
builder.AddJJMasterDataWeb()
.WithMongoDBDataDictionaryRepository(mongo =>
{
    mongo.ConnectionString = "mongodb://localhost:27017";
    mongo.CollectionName = "metadata";
    mongo.DatabaseName = "jjmasterdata";
});
```
## Development Setup
### Creating a MongoDB database using a Docker container

1. Pull the image
```sh
docker pull mongo
```
2. Run the container<br>
Obs: On --name you can put whatever name you want it. The convention for the port is 27017
```sh
docker run -d -p 27017:27017 --name mongodb mongo:latest
```

3. Check the port your database is running. 
```sh
docker ps
```
After that, everything is done, MongoDB will create your database at runtime 😄!
