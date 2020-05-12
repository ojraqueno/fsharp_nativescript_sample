module Features

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Data
open Utilities
open WebModel

let getToken =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let validate (request : GetTokenRequest) =
            let emailDoesNotExist email = not <| emailExists email
            let isInvalid email password =
                let salt, hash = getSaltHash email
                not <| isValidPassword password salt hash

            request |> validateFromRules
                [|
                    (fun m -> m.Email |> isNothing),            "Email is empty"
                    (fun m -> m.Email |> emailDoesNotExist),    "User not found"
                    (fun m -> m.Password |> isNothing),         "Password is empty"
                    (fun m -> m.Password |> isInvalid m.Email), "Invalid Password"
                |]

        let handle (request : GetTokenRequest) =
            let userId = getUserIdByEmail request.Email
            let settings = ctx.GetService<IConfiguration>()
            let secret = string <| settings.GetValue "secret"
            json <| generateToken userId secret

        task {
            let! request = ctx.BindJsonAsync<GetTokenRequest>()
            return! (request |> validateAndHandle validate handle) next ctx
        }

let register =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let validate (request : RegisterRequest) =
            request |> validateFromRules
                [|
                    (fun m -> m.Email |> isNothing),        "Email is empty"
                    (fun m -> m.Email |> isLongerThan 256), "Email is too long"
                    (fun m -> m.Email |> emailExists),      "User exists"
                    (fun m -> m.Password |> isNothing),     "Password is empty"
                |]

        let handle (request : RegisterRequest) =
            let userId = newGuid ()
            let salt, hash = createSaltHash request.Password
            insertUser userId request.Email request.Email salt hash
            setStatusCreated () >=> json userId

        task {
            let! request = ctx.BindJsonAsync<RegisterRequest>()
            return! (request |> validateAndHandle validate handle) next ctx
        }