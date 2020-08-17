# RestfulTestful
TIwPR project - using any technology supporting RESTful services design such service

| URI | GET | POST | PUT | PATCH | DELETE |
| --- | --- | --- | --- | --- | --- |
| /api/registration | account list | add account | X | X | X |
| /api/registration/{id} | account information | X | update account | partial update | deactivate account |
| /api/clients | client list | add client | X | X | X |
| /api/clients/{id} | client information with all sales | X | update client | partial update | archieve client and all his sales |
| /api/sales | get all sales with client and product information | add new sale | X | X | X |
| /api/sales/{id} | get sale information | X | update sale | partial update | archieve sale/delete archieved sale |
| /api/product | product list | add product | X | X | X |
| /api/product/{id} | product information | X | update product | partial update | discontnue product |
| /token | authorization token | X | X | X | X |
