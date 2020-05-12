module Data

open System
open FSharp.Data

// Cache
// ---------------------------------

let emailUserIdMap = new System.Collections.Generic.Dictionary<string, Guid>()

// Connection String
// ---------------------------------

[<Literal>]
let connectionString =
    @"Data Source=localhost\SQLEXPRESS;Initial Catalog=FSharp.Database;Integrated Security=True"

// SQL Statements
// ---------------------------------

[<Literal>]
let emailExistsQuery =
    "SELECT COUNT(*) FROM Users WHERE Email = @email"

let emailExists email =
    use cmd = new SqlCommandProvider<emailExistsQuery, connectionString>(connectionString)
    let result = cmd.AsyncExecute(email) |> Async.RunSynchronously

    match result |> Seq.cast<int option> |> Seq.head with
    | Some count -> count >= 1
    | None -> false

[<Literal>]
let insertUserQuery =
    "INSERT INTO Users (Id, Email, Username, PasswordSalt, PasswordHash) VALUES (@id, @email, @username, @passwordSalt, @passwordHash)"

let insertUser id email username passwordSalt passwordHash =
    use cmd = new SqlCommandProvider<insertUserQuery, connectionString>(connectionString)
    cmd.AsyncExecute(id, email,username, passwordSalt, passwordHash) |> Async.RunSynchronously |> ignore

[<Literal>]
let getSaltHashQuery =
    "SELECT [PasswordSalt], [PasswordHash] FROM [Users] WHERE [Email] = @email"

let getSaltHash email =
    use cmd = new SqlCommandProvider<getSaltHashQuery, connectionString>(connectionString)
    let result = cmd.AsyncExecute(email) |> Async.RunSynchronously |> Seq.head

    let salt =
        match result.PasswordSalt with
        | None -> raise (new InvalidOperationException())
        | Some value -> value

    let hash =
        match result.PasswordHash with
        | None -> raise (new InvalidOperationException())
        | Some value -> value

    salt, hash

[<Literal>]
let getUserIdByEmailQuery =
    "SELECT [Id] FROM [Users] WHERE [Email] = @email"

let getUserIdByEmail email =
    match emailUserIdMap.TryGetValue email with
    | true, guid -> guid
    | false, _ ->
        use cmd = new SqlCommandProvider<getUserIdByEmailQuery, connectionString>(connectionString)
        let result = cmd.AsyncExecute(email) |> Async.RunSynchronously |> Seq.head

        emailUserIdMap.Add(email, result)

        result