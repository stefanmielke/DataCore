# DataCore - Micro ORM for C#

DataCore's goal is to be a FOSS alternative to other ORMs. Focusing on simplicity and developer control over the queries and the database.

It uses Dapper for SQL execution and creates a layer on top of it to ease the query creation and database maintenance.

## Download

For now there's no Nuget version, so you can download/clone and build the current version from GitHub.

## Supported Databases

* SQLite
* SQL Server
* Oracle DB
* Postgres (planned for Beta)
* MySQL (planned for Beta)

Each database has its own project. So you can only add what you'll use.

## Usage

Each class represents one table and one table only.

Create a database for a connection using the desired provider and then you can use all methods from it.

### Small Example

```csharp
class User
{
  public int Id { get; set; }
  public string Name { get; set; }
}

using (var connection = new SQLiteConnection("Data Source=:memory:"))
{
    var db = new SqliteDatabase(connection);

    db.CreateTableIfNotExists<User>();
    
    db.Insert(new User { Id = 1, Name = "Test User" });

    var user = db.Select<User>(u => u.Id == 1);

    connection.Close();
}
```

### Simple Selects

#### Select with Where
```csharp
db.Select<User>(u => u.Id == 1);
```

#### Select one record with Where
```csharp
db.SelectSingle<User>(u => u.Id == 1);
```

#### Select By Id
```csharp
db.SelectById<User>(1);
```

#### Select By Ids
```csharp
db.SelectById<User>(1, 2, 3);
```

### Query object

You can use the Query class to create more complex queries.

```csharp
var query = db.From<User>(); // creates query object

// create query using object

var result = db.Select(query); // executes the query
var result = db.Select<MaxUser>(query); // returning other class (when changing field names)
```

### Joins

```csharp
var query = db.From<User>().Join<Address>((u, a) => u.Id == a.UserId);
var result = db.Select(query);
```
```csharp
var query = db.From<User>()
              .LeftJoin<Address>((u, a) => u.Id == a.UserId)
              .Where<Address>(a => a.Street.Like("Avenue%"));

var result = db.Select(query);
```
```csharp
// note: not supported by SQLite
var query = db.From<User>().RightJoin<Address>((u, a) => u.Id == a.UserId);
var result = db.Select(query);
```

### Group By

```csharp
var query = db.From<User>().GroupBy(u => u.Name).Count();
var result = db.Select(query);
```
```csharp
var query = db.From<User>().GroupBy(u => new { u.Name, u.Age }).Count();
var result = db.Select(query);
```
```csharp
var query = db.From<User>().GroupBy(u => u.Name).Having(u => u.Age.Sum() > 100).Count();
var result = db.Select(query);
```

### Select Columns

```csharp
var query = db.From<User>().Select(u => u.Id);
var result = db.Select(query);
```
```csharp
var query = db.From<User>().Select(u => new { u.Id, u.Name });
var result = db.Select(query);
```
```csharp
var query = db.From<User>().Select(u => new { u.Id, u.Name.Lower().As("Name") });
var result = db.Select(query);
```

### Extensions

You can use these extensions to use some SQL methods on your queries.

```csharp
var query = db.From<User>().Select(u => u.Id.TrimSql());
var result = db.Select(query);
```
```csharp
var result = db.Select<User>(u => u.Name.Like("%Test%"));
```
```csharp
var query = db.From<User>().GroupBy(u => u.Age.IsNull(0)).Select(u => new { u.Age, u.Name.Length().Min().As("MinName") });
var result = db.Select(query);
```

### Record Maintenance

#### Insert
```csharp
db.Insert<User>(new User { Id = 1, Name = "Test User" });
```

#### Update
```csharp
// updates user with Id = 1 with user data
db.Update<User>(user, u => u.Id == 1);
```

#### Update Only
```csharp
// only updates the name of the User with Id = 1
db.UpdateOnly<User>(user, t => t.Name, u => u.Id == 1);
```

#### Delete
```csharp
// delete user with Id = 1
db.Delete<User>(u => u.Id == 1);
```

#### Delete By Id
```csharp
// delete user with Id = 1
db.DeleteById<User>(1);
```

#### Delete By Ids
```csharp
// delete user with Ids 1, 2 and 3
db.DeleteById<User>(1, 2, 3);
```

### Database Schemas

You can use the following methods to create and drop parts of your database:

```csharp
db.CreateTableIfNotExists<User>();
db.DropTableIfExists<User>();

db.CreateColumnIfNotExists<User>(t => t.NewColumn);
db.DropColumnIfExists<User>(t => t.NewColumn);

db.CreateIndexIfNotExists<User>(t => new { t.Id, t.Name }, true, "IX_User_IdName_Unique");
db.DropIndexIfExists<User>("IX_User_IdName_Unique");

db.CreateForeignKeyIfNotExists<User, Address>(u => u.Id, a => a.UserId, "FK_User_Address");
db.DropForeignKeyIfExists<User>("FK_User_Address");
```

For automatic generation and Id usage, the following attributes can be used to decorate your properties.

```csharp
[Table("USER")]
class User
{
  [Column(isPrimaryKey: true, columnName: "User_ID"), Identity]
  public int Id { get; set; }
  
  [Index]
  public string Login { get; set; }
  
  public string Name { get; set; }

  [Column(isRequired: false)]
  public DateTime InsertDate { get; set; }
  
  [Ignore]
  public float Number { get; set; }
  
  [Reference(typeof(Address), "FK_User_Address")]
  public int AddressId { get; set; }
}
```

## Contributing

All push requests are welcome.

## Notes

Aditional usages can be found in the test project.
