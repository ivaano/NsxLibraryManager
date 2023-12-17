# Nsx Library Manager

## Description
This project helps to manage and visualize a nintendo switch library, organizing the files and creating a database with all the information.
I built this because I wanted to have a data grid that I could easily filter and sort to list my games.

## Features
- Web interface.
- Runs on Windows, Linux and Mac.
- Supports NSP, NSZ, XCI, XCZ files.
- Read a folder and analyze all the files.
- Extract metadata from the files, like name, size, titleId, publisher and Icon.
- Use titledb to aggregate more information.
- Create a database with all the information.
- Shows a list or grid of all your games.
- Filter by name, publisher, size, titleId, region, type, etc.
- Sort by name, publisher, size, titleId, region, type, etc.
- List missing DLC or updates for your games.
- Local TitleDb, no need to download it every time.

## Requirements
- dotnet 8.0
- prod.keys

## Install
- Download the latest release
- Extract the zip file
- Open `appsettings.json` and customize it to your needs:
  - `TitleDatabase`: Path where the db file is going to be stored, this is required and must end with `.db`. **Use a fast drive for this file, like a NVMe**.
  - `LibraryPath`: Path to your library.
  - `Recursive`: If true, it will search recursively in the library path.
  - `TitleDbPath`: Path where we are going to download titledb json Files to add them to the db.
  - `RegionUrl`: Url to download the region file.
  - `CnmtsUrl`: Url to download the cnmts file.
  - `VersionUrl`: Url to download the version file.
  - `Regions`: List of regions to download.
  - `ProdKeys`: Path to your prod.keys file, if this value is not set, program will look in the same folder as the executable, or you can put them in `$HOME/.switch/prod.keys`.

 ## Usage
- Run the `NsxLibraryManager.exe` file.
- Open your browser and go to [http://localhost:5000](http://localhost:5000).

## Screenshots
![Dashboard](./screenshots/dashboard.png)
![GameList](./screenshots/gamelist.png)
![GameCard](./screenshots/gamecard.png)
![Library](./screenshots/library.png)
![Detail](./screenshots/gamedetail.png)
![DetailDlcUpdates](./screenshots/gamedetail-2.png)
![MissingDLC](./screenshots/missingdlc.png)
![MissingUpdates](./screenshots/missingupdates.png)


## Credits
- [Libhac](https://github.com/Thealexbarney/LibHac) For the amazing library to read nintendo switch files.
- [Titledb](https://github.com/blawar/titledb) For the amazing database with all the information.
- [LiteDb](https://www.litedb.org) 
- [Radzen.Blazor](https://github.com/radzenhq/radzen-blazor) 
