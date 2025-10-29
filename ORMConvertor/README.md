A web-based tool for translating accross different .NET ORMs.

## Service boundaries

The solution is organised as a small microservice ecosystem:

* **Gateway (`ORMConvertorAPI`)** – hosts the Angular frontend and proxies API calls via YARP.
* **TranslationService** – performs ORM-to-ORM conversion requests.
* **MetadataService** – serves required content metadata and sample snippets.
* **AdvisorService** – runs benchmarking and optimisation workflows.

Each service can be run independently during development or together via the provided Docker Compose manifest.

# Deployment
The application can be run in two main ways. The more user-friendly approach is to open the solution file `ORMConvertor.sln` in Visual Studio. `ORMConvertorAPI` has to be set as the startup project, if not selected by default. The app can be launched in `Debug` configuration for development or compiled in `Release` configuration for production. Only the `http` launch profile defined in `ORMConvertorAPI/Properties/launchSettings.json` file has been configured and tested. The application can be started with `CTRL + F5` to run without debugging or `F5` to launch with the debugger. A browser window should open automatically.

Alternatively, the application can be started using .NET CLI using the following commands:
```sh
dotnet run --configuration Release --launch-profile http --project ORMConvertorAPI/ORMConvertorAPI.csproj
```
This approach does not open a browser automatically. Instead, the local URL is printed to the console (typically [http://localhost:5072/](http://localhost:5072/)).

Tests can be executed via the Visual Studio's Test Explorer window or with a .NET CLI command:
```sh
dotnet test Tests/Tests.csproj --configuration Release
```
The output displays the results of succeeded and failed tests. Tests are also run automatically on each commit using a GitHub Actions pipeline. The pipeline configuration is located in the `.github` folder at the root of the repository.

The Angular frontend is precompiled and served by the ASP.NET web server running the API. To prepare the frontend, its source files must be compiled and copied to the `wwwroot` directory, from which they are served as static files. This process is performed by executing the following commands in the `ORMConvertorAPI/frontend` directory.
```sh
npm install
ng build --configuration "production" --base-href "/" --deploy-url "/" && rmdir /s /q "..\wwwroot" && mkdir "..\wwwroot" && xcopy /s /e /y "dist\browser\*" "..\wwwroot\"
```

For Linux:
```sh
npm install
ng build --configuration "production" --base-href "/" --deploy-url "/" && rm -rf "../wwwroot" && mkdir "../wwwroot" && cp -r dist/browser/* ../wwwroot/
```

The frontend is served by the backend application, there is no need to initialize it separately.
