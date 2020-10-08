# PlayerWallet Service
REST API Project contains `Seed()` function that pre-populates the SQLite database with a Player with Guid `11111111-1111-1111-1111-111111111111` and his Wallet ~~with some transactions~~.

## Swagger

After running the PlayerWalletAPI project a swagger can be accessed at https://localhost:5001/swagger/index.html

## Architecture

Individual parts are divided into projects:

### PlayerWalletAPI
ASP.Net Core Rest API (non-MVC) project with Swagger (Swashbuckle) and XMLDoc.

### PlayerWalletContext
EntityFramework Core IDBContext code-first implementation used as Repository and UnitOfWork.
Uses SQLite for normal operation. 

Includes 2 more constructors:
- one for InMemory mocks (used by the xUnit Test project)
- one for CLI (`dotnet ef` commands to generate migrations and perform them)

### PlayerWalletTests
xUnit Test project with MS Visual Studio integration using ShouldLy extension.

Functional requirements:

All idempotent operations on Entities require clients to provide unique `TransactionId` in form of a GUID.
PUT requests to API controller methods are using AOP validations (using `ActionFilterAttribute`) to guarantee handling of repeated transactions and returning respective results.

Given the database backend is not the bottleneck it can be horizontally scaled by deploying many nodes. Each transaction modifying an Entity implements Optimistic Locking/Concurrency.
If the Entity being updated changed while the transaction was running a client receives HTTP status code 412 `Precondition Failed`. It is up to the client to repeat the transaction until it succeeds.
This is implemented using `RowVersion` database column. This however is not supported by SQLite so appropriate tests are missing.

Player's Wallet Entity also implements Memento pattern to save Events to a WalletLog table. This table is used to keep track of Event Stream on top of the Entity and to return the same ResultObject on repeated transactions.
