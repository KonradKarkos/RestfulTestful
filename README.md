# RestfulTestful
TIwPR project - using any technology supporting RESTful services design such service

| URI | GET | POST | PUT | PATCH | DELETE |
| --- | --- | --- | --- | --- | --- |
| /api/employees | employee list | add employee | X | X | X |
| /api/employees/{id} | employee information | X | update employee | partial update | deactivate/delete employee |
| /api/clients | client list | add client | X | X | X |
| /api/clients/{id} | client information with all sales | X | update client | partial update | archieve client and all his sales/delete archieved client |
| /api/sales | get all sales with client and product information | add new sale | X | X | X |
| /api/sales/{id} | get sale information | X | update sale | partial update | archieve sale/delete archieved sale |
| /api/products | product list | add product | X | X | X |
| /api/products/{id} | product information | X | update product | partial update | discontinue product/delete discontinued product |
| /token | X | get authorization token as response | X | X | X |
| /api/MergeClients/{id}?clientToAbsorbID={clientToAbsorbID} | X | X | transfer all sales to client from clientToAbsorb and delete clientToAbsorb | X | X |


All responses are instances of native classes from System.Web.Http.Results namespace ( https://docs.microsoft.com/en-us/previous-versions/aspnet/dn314678(v=vs.118) )

Where it was possible (due to, for example, making response too big) the response entities will also contain hypermedia fields ( https://en.wikipedia.org/wiki/HATEOAS ).

Uncomment db.Insert line in DataBaseConfig.cs before first use. Other way it will not be possible to obtain authorization token and use the service.