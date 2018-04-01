# DataCore - Micro ORM for C#

DataCore's goal is to be a FOSS alternative to other ORMs. Focusing on simplicity and developer control over the queries and the database.

It uses Dapper for SQL execution and creates a layer on top of it to ease the query creation and database maintenance.

## Download

For now there's no Nuget version, so you have to download/clone and build the current version from GitHub.

## Supported Databases

* SQLite
* SQL Server
* Oracle DB
* Postgres
* MariaDB (through MySQL, full support)
* MySQL (partial - no index support - planned for full release)

Each database has its own project. You can only add what you'll use.

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

using (var db = new DataCoreDatabase(new SqliteDatabase(), "Data Source=:memory:"))
{
    db.CreateTable<User>();
    
    db.Insert(new User { Id = 1, Name = "Test User" });

    var user = db.Select<User>(u => u.Id == 1);
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
```csharp
var query = db.From<User>()
              .LeftJoin<Address>((u, a) => u.Id == a.UserId)
              .GroupBy(u => u.Name)
              .GroupBy<Address>(a => a.Street)
              .Having(u => u.Age.Sum() > 100)
              .Having<Address>(a => a.Sum.Number.Count() > 2)
              .Count();
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
```csharp
var query = db.From<User>().Join<Address>((u, a) => u.Id == a.UserId)
              .Select(u => u.Id).Select<Address>(a => a.Street);
var result = db.Select(query);
```

### Select Exists

```csharp
var query = database.From<User>().Where(u => u.Id == 1);
bool exists = database.Exists(query);
```
```csharp
bool exists = database.Exists<User>(u => u.Id == 1);
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

#### All available extensions
```
Sum()
Min()
Max()
Average()
Count()
Between(start, end)
In(value1, value2, ...)
Like(string)
TrimSql()
Length()
Upper()
Lower()
IsNull(otherValue)
Cast<To>()
As(alias)
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
// databases (does not work for OracleDB)
db.CreateDatabase("test_db");
db.CreateDatabaseIfNotExists("test_db");

db.DatabaseExists("test_db"); // returns true

db.DropDatabaseIfExists("test_db");
db.DropDatabase("test_db");

// tables
db.CreateTable<User>();
db.CreateTables(typeof(User), typeof(Address));

db.CreateTableIfNotExists<User>();
db.CreateTablesIfNotExists(typeof(User), typeof(Address));

db.TableExists<User>(); // returns true

db.DropTable<User>();
db.DropTables(typeof(User), typeof(Address));

db.DropTableIfExists<User>();
db.DropTablesIfExists(typeof(User), typeof(Address));

db.DropAndCreateTable<User>();
db.DropAndCreateTables(typeof(User), typeof(Address));

// columns
db.CreateColumn<User>(t => t.NewColumn);
db.CreateColumnIfNotExists<User>(t => t.NewColumn);

db.ColumnExists<User>("NewColumn"); // returns true
db.ColumnExists<User>(t => t.NewColumn); // returns true

db.DropColumn<User>(t => t.NewColumn);
db.DropColumnIfExists<User>(t => t.NewColumn);

// indexes
db.CreateIndex<User>(t => new { t.Id, t.Name }, true, "IX_User_IdName_Unique");
db.CreateIndexIfNotExists<User>(t => new { t.Id, t.Name }, true, "IX_User_IdName_Unique");

db.IndexExists<User>("IX_User_IdName_Unique"); // returns true
db.IndexExists<User>(t => new { t.Id, t.Name }); // returns true

db.DropIndex<User>("IX_User_IdName_Unique");
db.DropIndexIfExists<User>("IX_User_IdName_Unique");

// foreign keys
db.CreateForeignKey<User, Address>(u => u.Id, a => a.UserId, "FK_User_Address");
db.CreateForeignKeyIfNotExists<User, Address>(u => u.Id, a => a.UserId, "FK_User_Address");

db.ForeignKeyExists<User>("FK_User_Address"); // returns true
db.ForeignKeyExists<User, Address>(u => u.Id, a => a.UserId); // returns true

db.DropForeignKey<User>("FK_User_Address");
db.DropForeignKeyIfExists<User>("FK_User_Address");
```

For automatic generation and Id usage, the following attributes can be used to decorate your properties.

```csharp
[Table("USER")] // explicit table name
class User
{
  // set as primary key, with the column name, and with AutoIncrement
  [Column(isPrimaryKey: true, columnName: "User_ID"), Identity]
  public int Id { get; set; }
  
  // create an index when creating the table
  [Index]
  public string Login { get; set; }
  
  public string Name { get; set; }

  // set as nullable
  [Column(isRequired: false)]
  public DateTime InsertDate { get; set; }
  
  // ignore the field for db
  [Ignore]
  public float Number { get; set; }
  
  // create a foreign key to Address with the name provided
  [Reference(typeof(Address), "FK_User_Address")]
  public int AddressId { get; set; }
}
```

## Contributing

All push requests are welcome.

## Notes

Aditional usages can be found in the test project.
