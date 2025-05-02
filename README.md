# VisualFXVault.OrdersService

Orders Microservice for E-Commerce website related to VFX.

Built with Layered Architecture.

Used technologies:

- MongoDB
- .NET 9
- EF Core
- FluentValidations

## TODO

### Initial setup

- [x] Add Data Access Layer project
- [x] Add Business Logic Layer project
- [x] Add API Layer project

### Data Access Layer

- [x] Add `Order` Entity with the following properties:
  - `OrderId` (GUID, primary key)
  - `UserId` (GUID)
  - `OrderDate` (DateTime)
  - `TotalBill` (decimal)
  - Reference to `OrderItem` (`List<OrderItem>`)
- [x] Add `OrderItem` Entity with the following properties:
  - `ProductId` (GUID)
  - `UnitPrice` (decimal)
  - `Quantity` (int)
  - `TotalPrice` (decimal)
- [x] Add `IOrdersRepository` with the following methods:
  - `GetOrders()` - to retrieve all orders.
  - `GetOrderByCondition()` - to retrieve an order by a specific condition.
  - `AddOrder()` - to add a new order.
  - `UpdateOrder()` - to update an existing order.
  - `DeleteOrder()` - to delete an order by its ID.
- [x] Implement `IOrdersRepository` in the `OrdersRepository` class
- [x] Add `DependencyInjection` class
- [x] Create `MongoDbSettings` class
- [x] Inject `MongoDbClient`

### Business Logic Layer

- [x] Add DTOs
  - [x] `OrderResponse`: Contains data to return in API responses.
  - [x] `OrderAddRequest`: For adding a new order.
  - [x] `OrderUpdateRequest`: For updating an existing order.
  - [x] `OrderItemAddRequest`: For adding a new order item.
  - [x] `OrderItemUpdateRequest`: For updating an existing order item.
- [x] Add `IOrderService` abstraction. This service will:
  - Retrieve all orders or orders by specific conditions.
  - Add, update, and delete orders by interacting with the repository.
- [x] Implement `IOrderService` in `OrderService` class.
- [ ] Create `OrderAddRequestValidator`, `OrderUpdateRequestValidator`, `OrderItemAddRequestValidator`,`OrderItemUpdateRequestValidator` with FluentValidation
- [ ] Add mapping profiles for DTOs

### API Layer

- [ ] Implement `OrdersController` API endpoints:
  - [ ] `GET /api/orders`: Retrieve all orders.
  - [ ] `GET /api/orders/search/order-id/{orderId}`: Retrieve an order by ID.
  - [ ] `GET /api/orders/search/product-id/{productId}`: Retrieve orders containing a specific product.
  - [ ] `GET /api/orders/search/order-date/{orderDate}`: Retrieve orders by a specific order date.
  - [ ] `POST /api/orders`: Add a new order.
  - [ ] `PUT /api/orders`: Update an existing order.
  - [ ] `DELETE /api/orders/{orderId}`: Delete an order by ID.
- [ ] Add exception handling middleware. This middleware should:
  - Log exceptions.
  - Return a generic error response with a 500 status code if any unhandled exceptions occur.
- [ ] Add Scalar UI for OpenAPI documentation
