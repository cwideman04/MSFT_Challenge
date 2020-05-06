# Microsoft Challenge

## Goal:

My goal is to create a API to take in a user’s location in San Francisco and give back 5 of the nearest Food Trucks. As a learning/refresher exercise I want to go with Azure and make it serverless and scalable to be able to handle other locations. We in Atlanta also enjoy Food Trucks. Time frame for completion is ~ 3 hours. (with learning curve, it is going to be closer to ~ 5 hours)

## Implementation:

I opted to use an Azure Function for my development.  Azure Function are serverless and have multiple hosting plans as user load grows.

Some Benefits:
- Can pay only when your functions are running.
- Scale out automatically, even during periods of high load and in when code stops running.
- High availability.
- Can be Imported to Azure API Management, to help secure and manage all APIs
- Develop in Azure Portal or Publish directly with in Visual Studio

For the data layer I decided to use Azure SQL database. Azure SQL database have a serverless option for lower usage scenarios. Also, SQL databases have geography spatial data type and functions to quickly do Nearest Neighbor Analysis.

Some Benefits:
- Autoscales
- High Availability
- Automatic backups
- Supports spatial data and spatial indexes.

Tradeoffs
- For the consumption plan, there is no “always on” option for function apps. Also Azure serverless SQL Databases automatically pauses when inactive. Both off these can cause cold start latencies exceeding 45 seconds at times


## API Documentation
The access path is https://msftchallenge20200505163449.azurewebsites.net/api/FoodTrucks

**Parameters**
| Key | Description |
| ------------- | ------------- |
| Latitude | User’s Latitude.  Required and must be between - 90 and 90 degrees |
| Longitude | User’s Longitude. Required and must be between -180 and 180 degrees.|


For example: https://msftchallenge20200505163449.azurewebsites.net/api/FoodTrucks?latitude=37.8849902793065&longitude=-122.41567662907

```javascript
[
  {
    "locationid": 1336168,
    "Applicant": "Anas Goodies Catering",
    "FacilityType": "Truck",
    "Address": "500 FRANCISCO ST",
    "FoodItems": "Cold Truck: Sandwiches: Noodles:  Pre-packaged Snacks: Candy: Desserts Various Beverages",
    "block": "0042",
    "lot": "022",
    "schedule": "http://bsm.sfdpw.org/PermitsTracker/reports/report.aspx?title=schedule&report=rptSchedule&params=permit=19MFF-00052&ExportPDF=1&Filename=19MFF-00052_schedule.pdf",
    "DistanceMiles": 5.5138,
    "Location": "(37.8050495090589, -122.41433443694)"
  },
..............
]

```






## If I had more time!
- Add parameter for "return record count" with default value of 5.
- Lock down the implementation with available security features
- Use Azure API Management as the entry point. 
- Would like to explore Cosmos DB for the data layer and understand what kind of supports it has for spatial data.
- Create a front end using Azure Maps.
- Create an IOT component. Have each Truck give live updates of location. If truck breaks down or is late to address user will know. Can also add notification for saved food trucks when they arrive.


## Getting Started

### Prerequisites
To run this sample you will need the following:
- Azure Account 
- Azure SQL Database
- Visual Studio 2019

### Download the sample and load the data
- Clone the sample repo.
- Using the FoodTruck2.csv, create a table named "Mobile_Food" in an Azure SQL Database and load in the csv file. Make sure the last column “GeoLocation” is of type geography.
- To create a Spatial index on “GeoLocation” run the following SQL Query:


```sql
Create Spatial INDEX IX_FoodTruck_SpatialLocation
ON dbo.Mobile_Food (GeoLocation)
USING GEOGRAPHY_GRID
WITH (Data_Compression = PAGE);
```


- In Visual Studio add an Azure App Service Setting:
  - Name  = “sqldb_connection”
  - Value for both Local & Remote values should be your Azure Database Connection String

- Run the Application
