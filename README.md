# fsharp_nativescript_sample

### Steps to run the server code

##### With Visual Studio Community (recommended)

1. Download [Visual Studio Community](https://visualstudio.microsoft.com/vs/community/).
2. Check "ASP.NET and web development". during installation.
3. Download the [.NET Core SDK](https://dotnet.microsoft.com/download).
4. Create the database by publishing the included database project. Update the connection string in `Data.fs`.
5. On Visual Studio, select FSharp.Web instead of IIS Express.
6. Run the solution as you normally would in Visual Studio.

##### With Visual Studio Code

1. Download [Visual Studio Code](https://code.visualstudio.com/download).
2. Install the [Ionide-fsharp](https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp) extension.
3. Download the [.NET Core SDK](https://dotnet.microsoft.com/download).
4. Create the database by running the scripts in the database project. Update the connection string in `Data.fs`.
5. Install packages by running `dotnet restore`. Build the solution by running `dotnet build`.
6. Run the solution by running `dotnet run`.

### Steps to run the mobile code

##### With an attached Android phone

1. Install [nodejs](https://nodejs.org/en/).
2. Install the NativeScript CLI by running `npm install -g nativescript`.
3. TODO