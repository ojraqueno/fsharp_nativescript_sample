module Utilities

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Security.Cryptography
open System.Text
open Microsoft.AspNetCore.Http
open Microsoft.IdentityModel.Tokens

// http://www.aspheute.com/english/20040105.asp
let private createRandomSalt () : int = 
    let _saltBytes = Array.create 4 ( new Byte() )
    (new RNGCryptoServiceProvider()).GetBytes(_saltBytes)

    (((int _saltBytes.[0]) <<< 24) + ((int _saltBytes.[1]) <<< 16) + ((int _saltBytes.[2]) <<< 8) + (int _saltBytes.[3]))

// http://www.aspheute.com/english/20040105.asp
let private computeSaltedHash  (salt : int) (password : string) : string =
    // Create Byte array of password string
    let encoder = new ASCIIEncoding()
    let _secretBytes = encoder.GetBytes(password)
    
    // Create a new salt
    let _saltBytes = Array.create 4 ( new Byte() )
    _saltBytes.[0] <- (byte)(salt >>> 24)
    _saltBytes.[1] <- (byte)(salt >>> 16)
    _saltBytes.[2] <- (byte)(salt >>> 8)
    _saltBytes.[3] <- (byte)(salt)

    // append the two arrays
    let toHash = Array.create (_secretBytes.Length + _saltBytes.Length) ( new Byte() )
    Array.Copy(_secretBytes, 0, toHash, 0, _secretBytes.Length);
    Array.Copy(_saltBytes, 0, toHash, _secretBytes.Length, _saltBytes.Length);

    let sha1 = SHA1.Create()
    let computedHash = sha1.ComputeHash(toHash);

    encoder.GetString(computedHash);

let createSaltHash (password : string) =
    let salt = createRandomSalt()
    let hash = password |> computeSaltedHash salt

    salt, hash

let isValidPassword (password : string) (salt : int) (hash : string) =
    let computedHash = password |> computeSaltedHash salt
    computedHash = hash

let generateToken (userId : Guid) (secret : string) =
    let claims = [|
        Claim(JwtRegisteredClaimNames.Sub, userId.ToString());
        Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]

    let expires = Nullable(DateTime.UtcNow.AddHours(1.0))
    let notBefore = Nullable(DateTime.UtcNow)
    let securityKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    let signingCredentials = SigningCredentials(key = securityKey, algorithm = SecurityAlgorithms.HmacSha256)

    let token =
        JwtSecurityToken(
            issuer = "fsharp.org",
            audience = "fsharp.org",
            claims = claims,
            expires = expires,
            notBefore = notBefore,
            signingCredentials = signingCredentials)

    JwtSecurityTokenHandler().WriteToken(token)

let getUserIdFromContext (ctx : HttpContext) =
    ctx.User.Claims
    |> Seq.filter (fun c -> c.Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
    |> Seq.map (fun c -> c.Value)
    |> Seq.exactlyOne
    |> Guid.Parse

let isNothing s =
    String.IsNullOrWhiteSpace s

let newGuid () =
    Guid.NewGuid()

let isLongerThan characterCount (s : string) =
    s.Length > characterCount