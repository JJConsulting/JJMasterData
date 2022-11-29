# JJMasterData.MongoDB

## Usage
1. Add the following section to your JJMasterData `IConfiguration` source.
```json
{
  "JJMasterData": {
    "MongoDB": {
      "MongoDBConnectionString": "mongodb://localhost:27017",
      "DatabaseName": "JJMasterData",
      "CollectionName": "metadata"
    }
  }
}
```
2. Add the following method to `JJServiceBuilder`:
```cs
builder.AddJJMasterDataWeb(settingsPath).WithMongoDB(mongo =>
{
    var section = builder.Configuration.GetSection("JJMasterData:MongoDB");
    mongo.ConnectionString = section.GetValue<string>("ConnectionString")!;
    mongo.CollectionName = section.GetValue<string>("CollectionName")!;
    mongo.DatabaseName = section.GetValue<string>("DatabaseName")!;
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
