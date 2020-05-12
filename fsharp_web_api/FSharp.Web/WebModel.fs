module WebModel

open System
open Giraffe

[<Literal>]
let Created = 201;

[<Literal>]
let BadRequest = 400

let setStatusCreated () =
    setStatusCode Created

type ValidationResult =
    | Valid
    | Invalid of string array

let validateFromRules (rules : (('a -> bool) * string) array) (model : 'a) =
    let firstError = rules |> Array.tryFind (fun (rule, _) -> rule model)

    match firstError with
    | None -> Valid
    | Some (_, error) -> Invalid [|error|]

let validateAndHandle validate handle request =
    match validate request with
    | Valid -> handle request
    | Invalid errors -> setStatusCode BadRequest >=> json errors
    
[<CLIMutable>]
type GetTokenRequest =
    {
        Email    : string
        Password : string
    }

[<CLIMutable>]
type RegisterRequest =
    {
        Email    : string
        Password : string
    }